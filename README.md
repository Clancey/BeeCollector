# BeeCollector
 
Welcome to my Smart BeeHive project!

## Goals

Build a smart hive that has integrated sensors!

* Check Tempurature around the hive
* Check Humidity
* Weight the hive
	* Measure honey production in the summer
	* Measure hive Health/food in the winter

## Tech Stack

* C# or Python in the hive for data collection
* Mqtt
* .NET
* Docker
* Axiom.co
* Azure SQL

## Parts List

### Main Hive Body

* SOC Your Choice Pick One!
	*  [Meadow by Wilderness Labs](https://store.wildernesslabs.co/collections/frontpage/products/meadow-f7-feather)
		*  C# 
		*  Built in Battery Charger!
		*  Built in Debugging!
	*  [Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html) 
		* Python
		* Cheap
	*  ESP32 Wi-Fi (Python)
		* Python
		* Cheap
		* Can get external WiFi Antenna 

* Sensors
	* Tempurature
		* DS18B20
			* Buy the waterproof version!
	* Tempurature with Humidity
		* DTH22 (This is what I am using. Minor code changes to use the others)
		* DHT11
		* DHT10/DHT12
	* Weight
		* hx711 (This is just the amplifier you still need sensors)
		* Load Cells
			* I went with bar strain. 
			* Half Bridge Strain Guage