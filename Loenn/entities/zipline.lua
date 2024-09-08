local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local zipline = {}


local handleTex = "isafriend/objects/zipline/handle"
local handleEndTex = "isafriend/objects/zipline/handle_end"

local baseColor = {0.8, 0.84, 0.9, 1.0}
local highlightColor = {0.55, 0.5, 0.6, 1.0}

local width = 16
local halfWidth = 8

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

--[[

-- Fix node selection rectangles after vertical move
function updateMoveSelection(room, entity, nodeIndex, selection, offsetX, offsetY)
    if nodeIndex > 0 then
        for _, node in ipairs(entity.nodes) do
            selection.y += offsetY
        end
    end
end

-- Zipline nodes should only move vertically when the main entity does
function zipline.onMove(room, entity, nodeIndex, offsetX, offsetY)
    if nodeIndex == 0 then
        for _, node in ipairs(entity.nodes) do
            node.y += offsetY
        end
    elseif nodeIndex > 0 and offsetY ~= 0 then
        return false
    end
end

-- Mostly copied from entities.moveSelection
function zipline.move(room, entity, node, offsetX, offsetY)
    if node == 0 then
        entity.x += offsetX
        entity.y += offsetY
        
        -- When the main entity moves vertically, the zipline nodes should follow
        for _, node in ipairs(entity.nodes) do
            node.y = entity.y
        end
        
        else
            local nodes = entity.nodes

            if nodes and node <= #nodes then
                local target = nodes[node]

                target.x += offsetX
                target.y += offsetY
            end
    end
end

function zipline.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local theme = string.lower(entity.theme or "normal")
    local themeData = themeTextures[theme] or themeTextures["normal"]

    local cogSprite = drawableSprite.fromTexture(themeData.nodeCog, entity)
    local cogWidth, cogHeight = cogSprite.meta.width, cogSprite.meta.height

    local mainRectangle = utils.rectangle(x, y, width, height)
    local nodeRectangle = utils.rectangle(centerNodeX - math.floor(cogWidth / 2), centerNodeY - math.floor(cogHeight / 2), cogWidth, cogHeight)

    return mainRectangle, {nodeRectangle}
end

local function addNodeSprites(sprites, entity, cogTexture, centerX, centerY, centerNodeX, centerNodeY)
    local nodeCogSprite = drawableSprite.fromTexture(cogTexture, entity)

    nodeCogSprite:setPosition(centerNodeX, centerNodeY)
    nodeCogSprite:setJustification(0.5, 0.5)

    local points = {centerX, centerY, centerNodeX, centerNodeY}
    local leftLine = drawableLine.fromPoints(points, ropeColor, 1)
    local rightLine = drawableLine.fromPoints(points, ropeColor, 1)

    leftLine:setOffset(0, 4.5)
    rightLine:setOffset(0, -4.5)

    leftLine.depth = 500
    rightLine.depth = 500

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, nodeCogSprite)
end

local function addBlockSprites(sprites, entity, blockTexture, lightsTexture, x, y, width, height)
    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, centerColor)

    local frameNinePatch = drawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    local lightsSprite = drawableSprite.fromTexture(lightsTexture, entity)

    lightsSprite:addPosition(math.floor(width / 2), 0)
    lightsSprite:setJustification(0.5, 0.0)

    table.insert(sprites, rectangle:getDrawableSprite())

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, lightsSprite)
end

function zipline.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    local centerX, centerY = x + halfWidth, y + halfHeight
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local theme = string.lower(entity.theme or "normal")
    local themeData = themeTextures[theme] or themeTextures["normal"]

    addNodeSprites(sprites, entity, themeData.nodeCog, centerX, centerY, centerNodeX, centerNodeY)
    addBlockSprites(sprites, entity, themeData.block, themeData.lights, x, y, width, height)

    return sprites
end



return zipline

--]]