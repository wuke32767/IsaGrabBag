module IsaGrabBagRewindCrystal

using ..Ahorn, Maple

@mapdef Entity "isaBag/rewindCrystal" RewindCrystal(x::Integer, y::Integer, oneUse::Bool=true)

const placements = Ahorn.PlacementDict(
	"Rewind Crystal (IsaGrabBag)" => Ahorn.EntityPlacement(
		RewindCrystal
	)
)

function Ahorn.selection(entity::RewindCrystal)
	x, y = Ahorn.position(entity)
	return Ahorn.getSpriteRectangle("objects/isafriend/rewind/idle00.png", x, y)
	
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RewindCrystal, room::Maple.Room)
	Ahorn.drawSprite(ctx, "isafriend/objects/rewind/idle00.png", 0, 0)
end

end