module IsaGrabBagArrowBlock

using ..Ahorn, Maple

@mapdef Entity "isaBag/arrowBlock" IsaArrowBlock(x::Integer, y::Integer, distance::Integer=16, inverted::Bool=false, movementRestriction::String="no_limit")

const placements = Ahorn.PlacementDict(
	"Friendly Kevin (IsaGrabBag)" => Ahorn.EntityPlacement(
		IsaArrowBlock,
		"rectangle"
	)
)

Ahorn.minimumSize(entity::IsaArrowBlock) = 24, 24
Ahorn.resizable(entity::IsaArrowBlock) = true, true

const axes = String[
	"no_limit",
	"horizontal",
	"vertical",
    "cardinal",
    "diagonal",
]

Ahorn.editingOptions(entity::IsaArrowBlock) = Dict{String, Any}(
	"movementRestriction" => axes
)

function Ahorn.selection(entity::IsaArrowBlock)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

const frameImage = Dict{String, String}(
    "no_limit" => "isafriend/objects/arrowblock/block00",
    "horizontal" => "isafriend/objects/arrowblock/block01",
    "vertical" => "isafriend/objects/arrowblock/block02",
    "cardinal" => "isafriend/objects/arrowblock/block03",
    "diagonal" => "isafriend/objects/arrowblock/block04"
)
const kevinColor = (72, 59, 105) ./ 255

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::IsaArrowBlock, room::Maple.Room)

    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    frame = frameImage[String(get(entity.data, "movementRestriction", "no_limit"))]
    faceSprite = Ahorn.getSprite("isafriend/objects/arrowblock/idle_face", "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    Ahorn.drawRectangle(ctx, 2, 2, width - 4, height - 4, kevinColor)
    Ahorn.drawImage(ctx, faceSprite, div(width - faceSprite.width, 2), div(height - faceSprite.height, 2))

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, 0, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, height - 8, 8, 24, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, 0, (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, width - 8, (i - 1) * 8, 24, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, 0, 0, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, 0, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, 0, height - 8, 0, 24, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, height - 8, 24, 24, 8, 8)
end

end