@{
    workItems  = @(
        @{
            id          = 100
            title       = "Automated setup of Azure Resources"
            type        = "User Story"
            area        = "Setup"
            sprintNo    = 1
            description = " " # cannot be empty
        }
        @{
            id          = 101
            pid         = 100
            title       = "ARM Template manually tested"
            type        = "Task"
            area        = "Setup"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 102
            pid         = 100
            title       = "Classic pipeline for ARM Template"
            type        = "Task"
            area        = "Setup"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 103
            pid         = 100
            title       = "YAML pipeline for ARM Template"
            type        = "Task"
            area        = "Setup"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 200
            title       = "Azure Function API to QA"
            type        = "User Story"
            area        = "API"
            sprintNo    = 1
            description = " " # cannot be empty
        }
        @{
            id          = 201
            pid         = 200
            title       = "Function code"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 202
            pid         = 200
            title       = "Pipeline for Function"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 203
            pid         = 200
            title       = "Function Unit tests"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 204
            pid         = 200
            title       = "Function Quality Gate"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 220
            title       = "Deploy Function"
            type        = "User Story"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 221
            pid         = 220
            title       = "Package Function"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 222
            pid         = 220
            title       = "Deploy Function to Slot"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 223
            pid         = 220
            title       = "Integration Tests"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 230
            pid         = 220
            title       = "Generazione Release notes"
            type        = "Task"
            area        = "API"
            sprintNo    = 1
            description = " "
        }
        @{
            id          = 300
            title       = "Azure Function API to Prod"
            type        = "User Story"
            area        = "API"
            sprintNo    = 2
            description = " " # cannot be empty
        }
        @{
            id          = 310
            pid         = 300
            title       = "Define Prod Environment"
            type        = "Task"
            area        = "API"
            sprintNo    = 2
            description = "Require green build and approval"
        }
        @{
            id          = 311
            pid         = 300
            title       = "Promote to Production"
            type        = "Task"
            area        = "API"
            sprintNo    = 2
            description = "Using slot swapping"
        }
        @{
            id          = 312
            pid         = 300
            title       = "Smoke Tests"
            type        = "Task"
            area        = "API"
            sprintNo    = 2
            description = " "
        }
        @{
            id          = 400
            title       = "New algorithm for Azure Function"
            type        = "User Story"
            area        = "API"
            sprintNo    = 2
            description = " " # cannot be empty
        }
        @{
            id          = 410
            pid         = 400
            title       = "Define Feature flag"
            type        = "Task"
            area        = "API"
            sprintNo    = 2
            description = " "
        }
        @{
            id          = 420
            pid         = 400
            title       = "Implement algorithm using Feature flag"
            type        = "Task"
            area        = "API"
            sprintNo    = 2
            description = " "
        }
        @{
            id          = 430
            pid         = 400
            title       = "Test in staging"
            type        = "Task"
            area        = "API"
            sprintNo    = 2
            description = " "
        }
        ###########################################
        @{
            id          = 800
            title       = "Implement IoT self-updating app"
            type        = "User Story"
            area        = "SelfUpdater"
            sprintNo    = 2
            description = " " # cannot be empty
        }
        @{
            id          = 810
            pid         = 800
            title       = "ARM Template"
            type        = "Task"
            area        = "SelfUpdater"
            sprintNo    = 2
            description = " "
        }
        @{
            id          = 820
            pid         = 800
            title       = "Setup pipeline"
            type        = "Task"
            area        = "SelfUpdater"
            sprintNo    = 2
            description = " "
        }
        @{
            id          = 830
            pid         = 800
            title       = "Auto-updating console application"
            type        = "Task"
            area        = "SelfUpdater"
            sprintNo    = 2
            description = " "
        }
        @{
            id          = 840
            pid         = 800
            title       = "Build & deploy pipeline"
            type        = "Task"
            area        = "SelfUpdater"
            sprintNo    = 2
            description = " "
        }
        @{
            id          = 850
            pid         = 800
            title       = "Test auto-update"
            type        = "Task"
            area        = "SelfUpdater"
            sprintNo    = 2
            description = " "
        }
    )
}
