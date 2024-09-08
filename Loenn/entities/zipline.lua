local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local handleTex = "isafriend/objects/zipline/handle"
local handleEndTex = "isafriend/objects/zipline/handle_end"

local baseColor = {0.8, 0.84, 0.9, 1.0}
local highlightColor = {0.55, 0.5, 0.6, 1.0}

local width = 16
local halfWidth = 8

local zipline = {}

zipline.name = "isaBag/zipline"
zipline.depth = -500
zipline.nodeVisibility = "never"
zipline.nodeLimits = {0, 2}
zipline.placements = {
    name = "default",
    data = {
        usesStamina = true
    }
}

function zipline.sprite(room, entity)
    local sprites = {}
    
    local x, y = entity.x or 0, entity.y or 0
    local minX = x
    local maxX = x
    
    if entity.nodes then
        for _, node in ipairs(entity.nodes) do
            if node.x < minX then
                minX = node.x
            elseif node.x > maxX then
                maxX = node.x
            end
        end
    end
    
    -- Draw zipline + end markers
    local ziplineRect = utils.rectangle(minX - halfWidth, y - 6, maxX - minX + width, 4)
    local ziplineSprite = drawableRectangle.fromRectangle("bordered", ziplineRect, baseColor, {0, 0, 0})
    local points = {minX - halfWidth, y - 3.5, maxX + halfWidth, y - 3.5} -- Line has a half-pixel offset for some reason
    local highlightSprite = drawableLine.fromPoints(points, highlightColor, 1)
    table.insert(sprites, ziplineSprite)
    table.insert(sprites, highlightSprite)
    
    local leftEndRect = utils.rectangle(minX - halfWidth - 3, y - halfWidth, 4, 8)
    local rightEndRect = utils.rectangle(maxX + halfWidth - 1, y - halfWidth, 4, 8)
    local leftEndSprite = drawableRectangle.fromRectangle("bordered", leftEndRect, baseColor, {0, 0, 0})
    local rightEndSprite = drawableRectangle.fromRectangle("bordered", rightEndRect, baseColor, {0, 0, 0})
    table.insert(sprites, leftEndSprite)
    table.insert(sprites, rightEndSprite)
    
    -- Draw main handle
    local mainSprite = drawableSprite.fromTexture(handleTex, entity)
    table.insert(sprites, mainSprite)    
    
    -- Draw node handle(s)
    if entity.nodes then
        for _, node in ipairs(entity.nodes) do
            local nodeSprite = drawableSprite.fromTexture(handleEndTex, entity)
            nodeSprite:addPosition(node.x - x, 0)
            table.insert(sprites, nodeSprite)
        end
    end
    
    return sprites
end

function zipline.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    
    local mainRect = utils.rectangle(x - halfWidth, y - halfWidth, 16, 17)
    local nodeRects = {}
    
    if entity.nodes then
        for _, node in ipairs(entity.nodes) do
            local nodeRect = utils.rectangle(node.x - halfWidth, y - halfWidth, 16, 17)    
            table.insert(nodeRects, nodeRect)
        end
    end
    
    return mainRect, nodeRects
end

-- Custom move function to limit node movement to the x-axis
function zipline.move(room, entity, node, offsetX, offsetY)
    local nodes = entity.nodes
    
    if node == 0 then
        entity.x += offsetX
        entity.y += offsetY
        
        -- When the main entity moves vertically, the zipline nodes should follow
        if nodes then
            for i, node in ipairs(entity.nodes) do
                if i <= #nodes then
                    node.y = entity.y
                end
            end
        end
        
        -- When a node moves, ignore any vertical movement
        else
            if nodes and node <= #nodes then
                local target = nodes[node]
                target.x += offsetX
            end
    end
end

-- The node selection should always match the height of the entity selection
function zipline.updateMoveSelection(room, entity, node, selection, offsetX, offsetY)
    if node == 0 then
        selection.x += offsetX
        selection.y += offsetY        
    else
        selection.x += offsetX
        selection.y = entity.y - halfWidth
    end
end

return zipline