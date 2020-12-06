[CmdletBinding()]
param (
    [string]
    $subscriptionName = "Microsoft Azure Sponsorship",
    [string]
    $organizationUrl = "https://dev.azure.com/workshop-2020-it/",
    [string]
    $parentProjectName = "docente",
    [string]
    $parentRepoName = "docente",
    [Parameter(Mandatory)]
    [string]
    $azureDevOpsPAT,
    [int]
    $numTeams = 12,
    [datetime]
    $baseDate = (Get-Date -Year 2020 -Month 12 -Day 9 -Hour 8 -Minute 0 -Second 0),
    [int]
    $sprintLength = 1
)

az extension add --name azure-devops

$env:AZURE_DEVOPS_EXT_PAT = $azureDevOpsPAT
az devops login  --organization $organizationUrl
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "me",$env:AZURE_DEVOPS_EXT_PAT)))
Remove-Item env:AZURE_DEVOPS_EXT_PAT

az account set -s $subscriptionName

$subscription = az account show | ConvertFrom-Json
$subscriptionId = $subscription.id

1..$numTeams | foreach {

    $idx = $_
    $projectName = "workshop_team${idx}"
    $resourceGroupName = "workshop_team${idx}"
    $repoName = "codice"
    $serviceConnectionName = "azure_${resourceGroupName}"

    $result = az devops project show  --project $projectName  --organization $organizationUrl | ConvertFrom-Json
    if ($result -eq $null) {
        $result = az devops project create  --name $projectName  --organization $organizationUrl  --process "Agile"  --visibility private | ConvertFrom-Json
    }
    $result.state
    $projectId = $result.id

    $result = az devops project show  --project $parentProjectName  --organization $organizationUrl | ConvertFrom-Json
    $parentProjectId = $result.id

    $result = az repos show  --repository $parentRepoName  --project $parentProjectName  --organization $organizationUrl | ConvertFrom-Json
    $parentRepoId = $result.id
    
    # fork the repo from the main project...
    $body = @"
{
    "name": "$repoName",
    "parentRepository": { "id": "$parentRepoId", "project": { "id": "$parentProjectId" } },
    "project": { "id": "$projectId", "name": "$projectName" }
}
"@
    $result = Invoke-RestMethod -Uri "${organizationUrl}${projectName}/_apis/git/repositories/?api-version=5.1" -Method Post -ContentType "application/json" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -Body $body
    $result.name

    $sp = az ad sp create-for-rbac --name "workshop_team${idx}_sp" | ConvertFrom-Json
    $sp.displayName

    $results = az devops service-endpoint list  --project $projectName  --organization $organizationUrl | ConvertFrom-Json
    $result = $results | where { $_.type -eq 'azurerm' -and $_.name -eq $serviceConnectionName }
    if ($result -eq $null) {
        $env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY = $sp.password
        $result = az devops service-endpoint azurerm create  --azure-rm-service-principal-id $sp.appId  --azure-rm-subscription-id $subscriptionId  --azure-rm-subscription-name $subscriptionName  --azure-rm-tenant-id $sp.tenant  --name $serviceConnectionName  --organization $organizationUrl  --project $projectName | ConvertFrom-Json
        Remove-Item env:AZURE_DEVOPS_EXT_AZURE_RM_SERVICE_PRINCIPAL_KEY
    }
    $result.name

    $result = az group create --resource-group $resourceGroupName --location westeurope | ConvertFrom-Json
    $result.properties.provisioningState

    $result = az role assignment create --role Contributor --assignee $sp.appId --resource-group $resourceGroupName | ConvertFrom-Json
    $result.scope
}


$result = az ad group create --display-name "g-workshop-2020-it" --mail-nickname "g-workshop-2020-it@example.com" --force false | ConvertFrom-Json
$groupId = $result.objectId
1..$numTeams | foreach {

    $idx = $_
    $resourceGroupName = "workshop_team${idx}"

    az role assignment create --role Contributor   --assignee-object-id $groupId  --assignee-principal-type Group  --resource-group $resourceGroupName
}

# DATA
1..$numTeams | foreach {

    $idx = $_
    $projectName = "workshop_team${idx}"

    $data = Import-PowerShellDataFile WorkItems.psd1
    $workItems =  $data.workItems

    # iterations
    1..3 | foreach {

        $sprintNo = $_

        $startDate = $baseDate.AddDays($sprintNo - 1).ToString("yyyy-MM-dd")
        $finishDate = $baseDate.AddDays($sprintNo).ToString("yyyy-MM-dd")
        az boards iteration project update --project $projectName --organization $organizationUrl  --start-date $startDate --finish-date $finishDate --path "\${projectName}\Iteration\Iteration ${sprintNo}"
    }

    # areas
    $workItems.area | sort -Unique | foreach {
        $area = $_
        az boards area project create  --project $projectName  --organization $organizationUrl   --path "\${projectName}\Area" --name $area
    }

    # make sub-areas visible in the backlog
    az boards area team update --team "${projectName} team" --project $projectName  --organization $organizationUrl  --path "\${projectName}"  --include-sub-areas

    # work items
    $idMap = @{}
    $workItems | foreach {
        $wiData = $_
        $result = az boards work-item create --project $projectName --organization $organizationUrl  --title $wiData.title --type $wiData.type --area "\${projectName}\$($wiData.area)"  --iteration "\${projectName}\Iteration $($wiData.sprintNo)" --description "$( $wiData.description )" | ConvertFrom-Json
        $result.id
        # fake --> true ID
        $idMap.Add($wiData.id, $result.id)
    }
    # add relations
    $workItems | where pid -ne $null | foreach {
        $child = $_
        $result = az boards work-item relation add --organization $organizationUrl --id $idMap[$child.id] --relation-type "parent" --target-id $idMap[$child.pid] | ConvertFrom-Json
        $result.id
    }
}

az devops logout