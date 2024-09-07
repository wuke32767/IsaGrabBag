local enums = require("consts.celeste_enums")

local coreWind = {}

coreWind.name = "isaBag/coreWindTrigger"
coreWind.fieldInformation = {
    patternHot = {
        options = enums.wind_patterns,
        editable = false
    },
    patternCold = {
        options = enums.wind_patterns,
        editable = false
    }
}

coreWind.placements = {
    name = "default",
    data = {
        patternHot = "Up",
        patternCold = "Down"
    }
}

return coreWind