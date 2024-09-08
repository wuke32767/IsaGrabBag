local variants = {
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
}

local enableStyles = {
	"Enabled",
	"Disabled",
	"EnabledPermanent",
	"DisabledPermanent",
	"EnabledTemporary",
	"DisabledTemporary",
	"Toggle",
	"SetToDefault"
}

local forceVariant = {}

forceVariant.name = "ForceVariantTrigger"
forceVariant.fieldInformation = {
    variantChange = {
        options = variants,
        editable = false
    },
    enableStyle = {
        options = enableStyles,
        editable = false
    }
}

return forceVariant