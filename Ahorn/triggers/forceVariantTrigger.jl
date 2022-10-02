module IsaGrabBagForceVariantTrigger

using ..Ahorn, Maple

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

function Ahorn.editingOptions(trigger::VariantTrigger)
	return Dict{String, Any}(
		"variantChange" => variants,
		"enableStyle" => variantMod
	)
end

end