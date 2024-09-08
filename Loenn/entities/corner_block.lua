local fakeTilesHelper = require("helpers.fake_tiles")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local cornerBlock = {}

cornerBlock.name = "isaBag/cornerBlock"
cornerBlock.depth = 0
cornerBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

function cornerBlock.sprite(room, entity)
    if entity.useTileset then
        return fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")(room, entity)
    else
        local fillColor = {0.8, 0.8, 0.8, 1.0}
        local borderColor = {0.4, 0.4, 0.4, 1.0}
        local rectangle = utils.rectangle(entity.x, entity.y, entity.width, entity.height)
        local drawRect = drawableRectangle.fromRectangle("bordered", rectangle, fillColor, borderColor)
        
        return drawRect:getDrawableSprite()
    end
end

return cornerBlock