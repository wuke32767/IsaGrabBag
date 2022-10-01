module IsaGrabBagCornerBlock

using ..Ahorn, Maple

@mapdef Entity "isaBag/cornerBlock" CornerBoostBlock(x::Integer, y::Integer, tiletype::String="1", useTileset::Bool=false)

Ahorn.minimumSize(entity::CornerBoostBlock) = 8, 8
Ahorn.resizable(entity::CornerBoostBlock) = true, true

Ahorn.editingOptions(entity::CornerBoostBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

function Ahorn.selection(entity::CornerBoostBlock)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CornerBoostBlock, room::Maple.Room)
    
    if get(entity.data, "useTileset", false)
        Ahorn.drawTileEntity(ctx, room, entity)
    else
        x, y = Ahorn.position(entity)
        
        width = Int(get(entity.data, "width", 32))
        height = Int(get(entity.data, "height", 32))
        
        Ahorn.drawRectangle(ctx, x, y, width, height, (0.8, 0.8, 0.8, 1.0), (0.4, 0.4, 0.4, 1.0))
    end
end

end