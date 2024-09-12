local booster = {}

booster.name = "Vithelpery/CountBooster"
booster.depth = -8500
booster.placements = {
    {
        name = "green",
        data = {
            count = 2,
            deadly = true,
        },
    },
}
booster.fieldInformation = {
    count = {
        fieldType = "integer",
    },
}

booster.texture = "objects/Vithelpery/countdownBooster/icon"

return booster