module IsaGrabBagDreamSpinner

using ..Ahorn, Maple

@mapdef Entity "isaBag/dreamSpinner" DreamSpinner(x::Integer, y::Integer, useOnce::Bool=false)

const placements = Ahorn.PlacementDict(
	"Dream Spinner (IsaGrabBag)" => Ahorn.EntityPlacement(
		DreamSpinner
	),
	"Dream Spinner (One Use) (IsaGrabBag)" => Ahorn.EntityPlacement(
		DreamSpinner,
		"rectangle",
		Dict{String, Any}(
			"useOnce" => true
		)
	)
)

function Ahorn.selection(entity::DreamSpinner)
	x, y = Ahorn.position(entity)
	return Ahorn.getSpriteRectangle("isafriend/danger/crystal/fg_dreamspinner.png", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamSpinner, room::Maple.Room)
	Ahorn.drawSprite(ctx, "isafriend/danger/crystal/fg_dreamspinner.png", 0, 0)
end

end