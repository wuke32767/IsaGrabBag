module IsaGrabBagArrowBubble

using ..Ahorn, Maple

@mapdef Entity "isaBag/arrowBubble" ArrowBubble(x::Integer, y::Integer, direction::String="down")

const directions = String[
	"up",
	"down",
	"left",
	"right"
]

Ahorn.editingOptions(entity::ArrowBubble) = Dict{String, Any}(
	"direction" => directions
)

const placements = Ahorn.PlacementDict(
	"Arrow Bubble (Down, IsaGrabBag)" => Ahorn.EntityPlacement(
		ArrowBubble,
		"rectangle",
		Dict{String, Any}(
			"direction" => "down"
		)
	),
	"Arrow Bubble (Up, IsaGrabBag)" => Ahorn.EntityPlacement(
		ArrowBubble,
		"rectangle",
		Dict{String, Any}(
			"direction" => "up"
		)
	),
	"Arrow Bubble (Left, IsaGrabBag)" => Ahorn.EntityPlacement(
		ArrowBubble,
		"rectangle",
		Dict{String, Any}(
			"direction" => "left"
		)
	),
	"Arrow Bubble (Right, IsaGrabBag)" => Ahorn.EntityPlacement(
		ArrowBubble,
		"rectangle",
		Dict{String, Any}(
			"direction" => "right"
		)
	)
)

function Ahorn.selection(entity::ArrowBubble)
	x, y = Ahorn.position(entity)
	
	return Ahorn.getSpriteRectangle("isafriend/objects/booster/boosterdown00.png", x, y)
	
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ArrowBubble, room::Maple.Room)
	
	dir = String(get(entity.data, "direction", "down"))
	
	Ahorn.drawSprite(ctx, "isafriend/objects/booster/booster$(dir)00.png", 0, 0)
end

end