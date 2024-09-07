local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local arrowBlock = {}

local restrictionOptions = {
    None = "no_limit",
    Horizontal = "horizontal",
    Vertical = "vertical",
    Cardinal = "cardinal",
    Diagonal = "diagonal"
}

arrowBlock.name = "isaBag/arrowBlock"
arrowBlock.depth = 0
arrowBlock.warnBelowSize = {24, 24}
arrowBlock.fieldInformation = {
    movementRestriction = {
        options = restrictionOptions,
        editable = false
    }
}
arrowBlock.placements = {
    name = "default",
        data = {
            width = 24,
            height = 24,
            distance = 16,
            inverted = false,
            movementRestriction = "no_limit",
        }
}

local frameTextures = {
    no_limit = "isafriend/objects/arrowblock/block00",
    horizontal = "isafriend/objects/arrowblock/block01",
    vertical = "isafriend/objects/arrowblock/block02",
    cardinal = "isafriend/objects/arrowblock/block03",
    diagonal = "isafriend/objects/arrowblock/block04"
}

local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

local kevinColor = {72 / 255, 59 / 255, 105 / 255}
local faceTexture = "isafriend/objects/arrowblock/idle_face"

function arrowBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local restriction = entity.movementRestriction or "no_limit"
    
    local frameTexture = frameTextures[restriction] or frameTextures["no_limit"]
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, kevinColor)
    local faceSprite = drawableSprite.fromTexture(faceTexture, entity)

    faceSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    faceSprite:setScale(1, entity.inverted and -1 or 1)

    local sprites = ninePatch:getDrawableSprite()

    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, faceSprite)

    return sprites
end

return arrowBlock