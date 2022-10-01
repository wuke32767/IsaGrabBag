module IsaGrabBagEntities

using ..Ahorn, Maple
@mapdef Entity "isaBag/dreamSpinner" DreamSpinner(x::Integer, y::Integer, useOnce::Bool=false)
@mapdef Entity "isaBag/dreamSpinFake" DreamSpinnerFake(x::Integer, y::Integer, useOnce::Bool=false)


const placements = Ahorn.PlacementDict(
	"Dream Spinner (IsaGrabBag)" => Ahorn.EntityPlacement(
		DreamSpinner
	),
	"Dream Spinner Fake (IsaGrabBag)" => Ahorn.EntityPlacement(
		DreamSpinnerFake
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
function Ahorn.selection(entity::DreamSpinnerFake)
	x, y = Ahorn.position(entity)
	return Ahorn.getSpriteRectangle("isafriend/danger/crystal/fg_dreamspinner_fake.png", x, y)
	
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamSpinner, room::Maple.Room)
	Ahorn.drawSprite(ctx, "isafriend/danger/crystal/fg_dreamspinner.png", 0, 0)
end
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamSpinnerFake, room::Maple.Room)
	Ahorn.drawSprite(ctx, "isafriend/danger/crystal/fg_dreamspinner_fake.png", 0, 0)
end

end