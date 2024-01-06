-----------------------------------------------------------------------------------------------------------
---- USAGE ------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- This script provides a physics addition where hitting speed pads will increase your top speed, which is
--  reset every lap and decreased when you collide with a wall.

-- Use the Create Variables From Script button in the Lua Runner to be given the following variables:
-- maxSpeed         : The total speed increase as a force (mass*distance/time^2) once all possible speed
--                    pads have been hit. This is the same application as the ships regular max speed stat.

-- maxPadsPerLap    : The number of pads that needs to be hit in a lap to reach the total maximum speed
--                    increase.

-- debugPrint       : Whether to log out debug messages to help with tuning the values. You can access logs
--                  : in-game by pressing F9 and then opening the logger window.

-----------------------------------------------------------------------------------------------------------
---- UNITY VARIABLES --------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- var maxSpeed Float
-- var maxPadsPerLap Integer
-- var debugPrint Boolean

-----------------------------------------------------------------------------------------------------------

-- a table which contains all of the speed pads that were flew over in the ships current lap
speedPads = {}
-- a table which contains all of the speed tiles that were flew over in the ships current lap
speedTiles = {}

-- the number of speed pads / tiles that were flew over in the ships current lap
hitPads = 0

-- whether the ship had a heavy collision in the previous physics frame
wasColliding = false

-- if any of the variables haven't been created in Unity, set them to default values.
if (maxSpeed == nil) then maxSpeed = 0 end
if (maxPadsPerLap == nil) then maxPadsPerLap = 0 end
if (debugPrint == nil) then debugPrint = false end

-- FixedUpdate is where the the physics engine is updated, so we want to apply physics forces in here.

-- First we'll check if the ship has started colliding since the last physics frame. If it has, we'll decrement the hit pads.
-- Then we'll calculate a ratio for the number of unique speed pads hit in the current lap and then multiply the maxSpeed
--  set in the scripts variables by this ratio.

-- We can then use the ships engineAccel variable to apply the additional top speed force along the ships forward
--  axis. This is how the ships thruster force is applied internally, so this scales our force correctly with the
--  internal speed of the ship.
function FixedUpdate(deltaTime)
		-- collision check
		isColliding = ship.PysSim.isColliding
		if (isColliding ~= wasColliding) then
			CollisionUpdate(isColliding)
		end

		-- apply additional speed force
		additionalTopSpeed = Mathf:Clamp01(hitPads / Mathf:Max(maxPadsPerLap, 1)) * maxSpeed
		body.AddRelativeForce(Vector3:forward * ship.PysSim.engineAccel * additionalTopSpeed)
end

-- Checks if the ship has had a heavy collision with a wall after a collision state change and decrements the hit pads count
--  if it has.
function CollisionUpdate(isColliding)
	if (isColliding) then
		DecrementHitPads()

		if (debugPrint) then
			print(ship.ShipName .. " hit wall (" .. tostring(hitPads) .. " / " .. tostring(maxPadsPerLap) .. ")")
		end
	end

	wasColliding = isColliding
end

-- Decrements the number of hits pads with a lower clamp 0
function DecrementHitPads()
	hitPads = hitPads - 1
	if (hitPads < 0) then
		hitPads = 0
	end
end

-- Increments the number of hits pads with an upper clamp for maxPadsPerLap
function IncrementHitPads()
	hitPads = hitPads + 1
	if (hitPads > maxPadsPerLap) then
		hitPads = maxPadsPerLap
	end
end

-- When the ship has flew over a speed pad, check if the pad has already been flown over and increment the hitPads
--  value if it hasn't.
-- 
-- If it hasn't, we'll add the pad to the speedPads table so we don't count it again for the current lap.
function OnShipHitSpeedPad(pad)
	if (speedPads[pad] == nil) then
		speedPads[pad] = true

		IncrementHitPads()
	end

	if (debugPrint) then
		print(ship.ShipName .. " hit speed pad (" .. tostring(hitPads) .. " / " .. tostring(maxPadsPerLap) .. ")")
	end
end

-- When the ship has flew over a speed tile, check if the tile has already been flown over and increment the hitPads
--  value if it hasn't.
-- 
-- If it hasn't, we'll add the pad to the speedTiles table so we don't count it again for the current lap.
function OnShipHitSpeedTile(tile)
	if (speedTiles[tile] == nil) then
		speedTiles[tile] = true

		IncrementHitPads()
	end

	if (debugPrint) then
		print(ship.ShipName .. " hit speed tile (" .. tostring(hitPads) .. " / " .. tostring(maxPadsPerLap) .. ")")
	end
end

-- When the ship has completed a lap, we'll reset the pads and tiles tables and restore the hitPads value to zero.
function OnShipLapUpdate()
	speedPads = {}
	speedTiles = {}
	hitPads = 0

	if (debugPrint) then
		print(ship.ShipName .. " completed lap")
	end
end