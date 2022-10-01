module IsaGrabBagCoreWindTrigger

using ..Ahorn, Maple

@mapdef Trigger "isaBag/coreWindTrigger" CoreWindTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, patternHot::String="Up", patternCold::String="Down")

const placements = Ahorn.PlacementDict(
	"Core Wind Trigger (IsaGrabBag)" => Ahorn.EntityPlacement(
		CoreWindTrigger,
		"rectangle"
	)
)

function Ahorn.editingOptions(trigger::CoreWindTrigger)
	return Dict{String, Any}(
		"patternHot" => Maple.wind_patterns,
		"patternCold" => Maple.wind_patterns
	)
end

end