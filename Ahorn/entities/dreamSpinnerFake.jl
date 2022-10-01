module IsaGrabBagDreamSpinnerFake

using ..Ahorn, Maple

@mapdef Entity "isaBag/dreamSpinFake" DreamSpinnerFake(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
	"Dream Spinner Fake (IsaGrabBag)" => Ahorn.EntityPlacement(
		DreamSpinnerFake
	)
)

function Ahorn.selection(entity::DreamSpinnerFake)
	x, y = Ahorn.position(entity)
	return Ahorn.getSpriteRectangle("isafriend/danger/crystal/fg_dreamspinner_fake.png", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamSpinnerFake, room::Maple.Room)
	Ahorn.drawSprite(ctx, "isafriend/danger/crystal/fg_dreamspinner_fake.png", 0, 0)
end

end