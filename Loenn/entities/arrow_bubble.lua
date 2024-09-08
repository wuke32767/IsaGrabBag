local arrowBubble = {}

local directions = {
    Up = "up",
    Down = "down",
    Left = "left",
    Right = "right"
}

arrowBubble.name = "isaBag/arrowBubble"
arrowBubble.depth = -8500
arrowBubble.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    }
}

arrowBubble.placements = {}

for _, dir in pairs(directions) do
    table.insert(arrowBubble.placements, {
        name = dir,
        data = {
            direction = dir
        }
    })
end

function arrowBubble.texture(room, entity)
    return string.format("isafriend/objects/booster/booster%s00", entity.direction)
end

return arrowBubble