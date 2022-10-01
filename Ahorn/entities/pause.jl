module IsaGrabBagPauseCrystal

using ..Ahorn, Maple

@mapdef Entity "isaBag/pauseCrystal" PauseCrystal(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
	"Pause Crystal (IsaGrabBag)" => Ahorn.EntityPlacement(
		PauseCrystal
	)
)

function Ahorn.selection(entity::PauseCrystal)
	x, y = Ahorn.position(entity)
	return Ahorn.getSpriteRectangle("objects/isafriend/pause/idle00.png", x, y)
	
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PauseCrystal, room::Maple.Room)
	Ahorn.drawSprite(ctx, "isafriend/objects/pause/idle00.png", 0, 0)
end

end