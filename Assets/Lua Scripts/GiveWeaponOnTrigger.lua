-- var pickupName String

-- check variables
if pickupName == nil then
    pickupName = "random"
end

-- give ship the pickup
function OnShipEnter(ship)
    if pickupName == "random" then
        PickupRegistry.GetRandomPickup(ship)
    else
        pickup = PickupRegistry.FindPickupByName(pickupName)
        PickupRegistry.GivePickupToShip(ship, pickup)
    end
end