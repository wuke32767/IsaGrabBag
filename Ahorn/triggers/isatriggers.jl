module IsaGrabBagTriggers

using ..Ahorn, Maple

@mapdef Trigger "CoreHeatWindTrigger" CoreWindTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, patternHot::String="Up", patternCold::String="Down")
@mapdef Trigger "ForceVariantTrigger" VariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, variantChange::String="Hiccups", enableStyle::String="EnabledPermanent")

const variants = String[
	"Hiccups",
	"InfiniteStamina",
	"Invincible",
	"InvisibleMotion",
	"LowFriction",
	"MirrorMode",
	"NoGrabbing",
	"PlayAsBadeline",
	"SuperDashing",
	"ThreeSixtyDashing",
	"DashAssist"
]
const variantMod = String[
	"Enabled",
	"Disabled",
	"EnabledPermanent",
	"DisabledPermanent",
	"EnabledTemporary",
	"DisabledTemporary",
	"Toggle",
	"SetToDefault"
]

const placements = Ahorn.PlacementDict(
	"Core Wind Trigger (IsaGrabBag)" => Ahorn.EntityPlacement(
		CoreWindTrigger,
		"rectangle"
	),
	"Force Variant Trigger (IsaGrabBag)" => Ahorn.EntityPlacement(
		VariantTrigger,
		"rectangle"
	)
)

function Ahorn.editingOptions(trigger::CoreWindTrigger)
	return Dict{String, Any}(
		"patternHot" => Maple.wind_patterns,
		"patternCold" => Maple.wind_patterns
	)
end

function Ahorn.editingOptions(trigger::VariantTrigger)
	return Dict{String, Any}(
		"variantChange" => variants,
		"enableStyle" => variantMod
	)
end

end