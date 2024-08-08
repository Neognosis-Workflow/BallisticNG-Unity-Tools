-----------------------------------------------------------------------------------------------------------
---- USAGE ------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- This script allows you to rotate something on the ship using the ships engine power. Intended use is for
--  something like a throttle in the cockpit.

-- This is a per-frame script. Set Update Tick Rate in Lua Runner to Unlocked!

-- Use the Create Variables From Script button in the Lua Runner to be given the following variables:
-- throttle		        : A reference to the object to rotate.
--
-- rotateAxis	 	    : The axis that the object will rotate around. Take note of the axis that goes along
-- 						  the desired rotation and set that axis's value to 1. Leave the others at 0.
--
-- rotateAmount			: The angle in degrees to rotate the object. For instance, setting this to 45
--                        will have the object rotate by 45 degrees when the engine power is maxed out.

-----------------------------------------------------------------------------------------------------------
---- UNITY VARIABLES --------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- var throttle Transform
-- var rotateAxis Vector3
-- var rotateAmount Float

if (rotateAxis == nil) then rotateAxis = Vector3:Forward end
if (rotateAmount == nil) then rotateAmount = 0 end
startRot = nil

function Start()
	-- cache the throttles starting rotation
	if (throttle ~= nil) then startRot = throttle.localRotation end
end

function Update(unscaledDeltaTime, deltaTime)
	-- don't do anything for AI / networked ships since we'll never see their cockpit internals
	if (ship.IsPlayer ~= true) then return end
	if (throttle == nil) then return end
	
	-- rotate the throttle
	throttle.localRotation = startRot * Quaternion:AngleAxis(rotateAmount * ship.PysSim.enginePower, rotateAxis)
end