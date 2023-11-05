-- var target Transform
-- var direction Vector3
-- var speed Float
-- var amount Float

-- check variables
canRun = true
if target == nil then
    print("<color=red>" .. scriptName.. " on " .. gameObject.name .. " script can't run (No Target)</color>")
    canRun = false
end

if direction == nil then
    direction = Vector3.up
end

if speed == nil then
    speed = 1.0
end

if ammount == nil then
    amount = 1.0
end

-- init data
if canRun then
    startPos = target.position
    moveDir = target.TransformDirection(direction)
end

-- update object
function Update(unscaledDeltaTime, deltaTime)
    if canRun then
        target.position = startPos + moveDir * Mathf.Sin(Time.timeSinceLevelLoad * speed) * amount
    end
end