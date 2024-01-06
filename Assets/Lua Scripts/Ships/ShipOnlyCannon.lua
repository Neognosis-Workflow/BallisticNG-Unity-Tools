-----------------------------------------------------------------------------------------------------------
---- USAGE ------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- This script will disable weapons on a ship and then give it a cannon at fixed intervals if it isn't
--  currently holding one. The number of bullets and the cooldown between dropping and picking up a new cannon
--  are customizable.

-- Use the Create Variables From Script button in the Lua Runner to be given the following variables:
-- bulletCount          : The number of bullets the cannon will have.
--
-- cooldownTime         : How long to wait in seconds between dropping a cannon and picking up a new one.

-----------------------------------------------------------------------------------------------------------
---- UNITY VARIABLES --------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- var bulletCount Int
-- var cooldownTime Float

-----------------------------------------------------------------------------------------------------------

-- if any of the variables haven't been created in Unity, set them to default values.
if (bulletCount == nil) then bulletCount = 15 end
if (cooldownTime == nil) then cooldownTime = 5 end

-- the cooldown timer will allow us to add short breaks between having the cannon
cooldownTimer = 0.0

-- the cannon doesn't send an event when all bullets have been fired, so we'll want to monitor this ourselves
prevPickup = nil

-- disable the ships weapons
ship.Settings.DAMAGE_PWR = -1

-- if the gamemode doesn't allow weapons then don't do anything
canRun = RaceManager:CurrentGamemode.Configuration.WeaponsAllowed

function Update(unscaledDeltaTime, deltaTime)
	if (not canRun or Race:AllShipsRestrained) then return end

	-- monitor the ships current pickup to determine when the cannon has been dropped or depleted so we can update the
	--  cooldown timer.
	if (prevPickup ~= ship.CurrentPickup) then
		if (prevPickup ~= nil and ship.CurrentPickup == nil) then cooldownTimer = cooldownTime end

		prevPickup = ship.CurrentPickup
	end

	if (ship.CurrentPickup == nil and Mathf:Approximately(cooldownTimer, 0)) then
		-- ballisticNG has static references to all of its pickups, but if you're working with a mod that hasn't exposed
		-- anything to Lua, you can also use FindPickupByName:GetPickupByName(pickupName)
		PickupRegistry.GivePickupToShip(ship, PickupRegistry:PickupCannon)

		-- give the cannon bulletCount bullets instead of the default 30.
		-- the type is automatically cast, so unlike C# we don't need to worry about making sure it's the PickupCannons
		--  type before accessing it
		ship.CurrentPickup.bullets = bulletCount
		ship.PickupDisplayText = ship.CurrentPickup.bullets
	end

	cooldownTimer = cooldownTimer - deltaTime
	if (cooldownTimer < 0) then cooldownTimer = 0 end
end