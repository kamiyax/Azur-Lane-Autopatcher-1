# Azur Lane - Purge the cheaters
Let's purge the cheaters in this kuso game.

## Vulnerable files
1. weapon_property.lua
   - **attribute:** damage, reload_max
2. enemy_data_statistics.lua
   - **attribute:** equipment_list, durability, durability_growth, antiaircraft, antiaircraft_growth, antisub, armor, ArmorGrowth, cannon, dodge, dodge_growth, hit, hit_growth, luck, luck_growth, reload, reload_growth, speed, speed_growth, torpedo, torpedo_growth
3. aircraft_template.lua
   - **attribute:** max_hp, hp_growth, accuracy, ACC_growth, attack_power, AP_growth, crash_DMG, speed

---

**Method 1:** Hash Check - Calculate and verify checksums and hashes of the vulnerable files.\
Calculate & Verify->If mismatch->Flag user as a cheater and send the data to the server->Profit

---

**Method 2:** Verify the content of vulnerable files.\
The autopatcher is using the most basic way to modify the vulnerable files; Regex Replace. Which means, all attributes bound to have the same value. We can easily track modified scripts by comparing specific attribute of each array.

Let's say, compare reload attribute of each weapon, then flag the user as a cheater and send the data to the server if all of them are the same.

---

**Method 3:** Rename attributes to numeric/store important attributes inside an array.\
This will at least made the autopatcher no longer functionable and confuse those who able to modify the content.

---

1. I don't need to explain everything since I'm sure the developers will get it, as they've resorted to the **Method 3** in the past (To secure **ship_data_statistics.lua**).

2. The game itself already has a HashChecker which its function is to verify the vulnerable files, and a function to verify battle data that will flag the user as a cheater if the data is abnormal by verifying the overall Damage, stats of ships, equipped weapons, etc. (I'm not really sure how they works)

3. Over 3 months have passed since the last time they updated JP, CN and KR base client.

---

I suspect the developers pretend to not know about the existence of cheaters and let them to roam. Currently, its JP, CN and KR server gives literally 0 fucks about cheaters. Except its EN server, for the sake of saving their image because gaijins are obnoxious. However, they only ban those who appear in EX rankings, which is not many because I got at least 6k+ traffic each day when I still actively distribute modified apk, which means there are at least 3k+ cheaters.

That's it, thanks for reading this useless information.
