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

function spring.flip(room, entity, h, v)
    local dir = entity.direction or "Up"
    if h then
        if dir == "Right" then
            entity.direction = "Left"
            return true
        elseif dir == "Left" then
            entity.direction = "Right"
            return true
        end
    elseif v then
        if dir == "Up" then
            entity.direction = "Down"
            return true
        elseif dir == "Down" then
            entity.direction = "Up"
            return true
        end
    end
    return false
end

function spring.rotate(room, entity, o)
    local dir = entity.direction or "Up"
    local order = {"Up", "Right", "Down", "Left"}
    local index = 0
    for i,_dir in ipairs(order) do
        if _dir == dir then
            index = i
            break
        end
    end
    local new_i = index + o
    entity.direction = order[(new_i - 1) % 4 + 1]
    return true
end

return spring