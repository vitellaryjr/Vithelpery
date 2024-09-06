local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local spring = {}
spring.name = "Vithelpery/BumpSpring"
spring.depth = -8501

local directions = {"Up", "Right", "Left", "Down"}
local bounceTypes = {"Bumper", "BumperCancel", "Spring", "SpringBoost"}
spring.placements = {}
for _,dir in ipairs(directions) do
    table.insert(spring.placements, {
        name = string.lower(dir),
        data = {
            direction = dir,
            bounceType = "BumperCancel",
            cooldown = 0.6,
        }
    })
end
spring.fieldInformation = {
    direction = {
        options = directions,
        editable = false,
    },
    bounceType = {
        options = bounceTypes,
        editable = false,
    },
}

function spring.sprite(room, entity, viewport)
    local sprite = drawableSprite.fromTexture("objects/Vithelpery/bumpSpring/idle00", entity)
    sprite:setJustification(0.5, 1)
    local dir = entity.direction or "Up"
    if dir == "Right" then
        sprite.rotation = math.pi/2
    elseif dir == "Left" then
        sprite.rotation = -math.pi/2
    elseif dir == "Down" then
        sprite.rotation = math.pi
    end

    return sprite
end

function spring.selection(room, entity)
    local dir = entity.direction or "Up"
    if dir == "Right" then
        return utils.rectangle(entity.x, entity.y - 8, 5, 16)
    elseif dir == "Left" then
        return utils.rectangle(entity.x - 5, entity.y - 8, 5, 16)
    elseif dir == "Down" then
        return utils.rectangle(entity.x - 8, entity.y, 16, 5)
    else
        return utils.rectangle(entity.x - 8, entity.y - 5, 16, 5)
    end
end

return spring