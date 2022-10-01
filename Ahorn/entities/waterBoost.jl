module IsaGrabBagWaterBoost

using ..Ahorn, Maple

@mapdef Entity "isaBag/waterBoost" WaterBoost(x::Integer, y::Integer, boostEnabled::Bool=true)

const placements = Ahorn.PlacementDict(
	"Water Boost Controller (IsaGrabBag)" => Ahorn.EntityPlacement(
		WaterBoost,
		"rectangle",
		Dict{String, Any}(
			"boostEnabled" => true
		)
	)
)

sprite = "isafriend/helperimage.png"

function Ahorn.selection(entity::WaterBoost)
	x, y = Ahorn.position(entity)
	return Ahorn.getSpriteRectangle(sprite, x, y)
	
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaterBoost, room::Maple.Room)
	Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end