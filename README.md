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

<img width="824" alt="image" src="https://user-images.githubusercontent.com/256046/229032407-c52b89fc-5e16-492c-941b-f1c32883c0f8.png">

* C# or Python in the hive for data collection
* Mqtt
* .NET
* Docker
* [Axiom.co](https://axiom.co/)
* Azure SQL


## TODO

* Build an API/ Mobile App
* Build a Smart Hive Cover
	* Camera
	* Humidity/Temp sensors
* More documentation

## Parts List

### Main Hive Body

* SOC Your Choice Pick One!
	*  [Meadow by Wilderness Labs](https://store.wildernesslabs.co/collections/frontpage/products/meadow-f7-feather) 
		*  <img src="https://user-images.githubusercontent.com/256046/229030914-3c27fc50-fd5f-45cb-b4c0-5cb062de4e49.png" width="200" />
		*  C# 
		*  Built in Battery Charger!
		*  Built in Debugging!
	*  [Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html)
		* <img src="https://user-images.githubusercontent.com/256046/229030461-859e5c04-5d1b-453d-940e-9beed4d26356.png" width="50" />
		* Python
		* Cheap
	*  ESP32 Wi-Fi (Python) 
		* <img src="https://user-images.githubusercontent.com/256046/229030829-82062396-c442-4c89-86a3-0e661b85f02a.png" width="50" />
		* Python
		* Cheap
		* Can get external WiFi Antenna 

* Sensors
	* Tempurature
		* DS18B20
			* Buy the waterproof version!
			* <img src="https://user-images.githubusercontent.com/256046/229031421-06aba076-0523-4526-aeb4-7078a82c1e48.png" width="100" />

	* Tempurature with Humidity
		* DTH22 (This is what I am using. Minor code changes to use the others)
			* <img width="100" alt="image" src="https://user-images.githubusercontent.com/256046/229031232-f62bac55-db9a-48a8-b6af-904ac634975a.png">
		* DHT11
		* DHT10/DHT12
	* Weight
		* hx711 (This is just the amplifier you still need sensors)
		* Load Cells
			* I went with bar strain. 
				* <img width="100" alt="image" src="https://user-images.githubusercontent.com/256046/229031552-a55b1538-0406-4c4f-ace0-4b2adcf6c929.png">
			* Half Bridge Strain Guage
				* <img width="100" alt="image" src="https://user-images.githubusercontent.com/256046/229031663-e58606fc-cf4f-4118-8cf1-b12f494f85cf.png">
