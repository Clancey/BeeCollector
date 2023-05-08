from machine import Pin, ADC
from time import sleep
from hx711 import HX711
from onewire import OneWire
from ds18x20 import DS18X20
import utime
import _thread
import machine,dht,math,network,urequests,ntptime,time,json
from machine import WDT
from umqtt.robust import MQTTClient

print("Hello BeeHive")  

wdt = WDT(timeout=8388) #timeout is in ms
wdt.feed() #resets countdown

pin = Pin("LED", Pin.OUT)
pin.toggle()
sleep(1)
pin.toggle()
#Config
wifiName = "***REMOVED***"
wifiPassword =  "***REMOVED***";
mqqtPort = 1883;
mqqtServer = "synology.local";
timeBetweenReadings = 60
beeHiveName = "Hive1"
#if the pin is >0 we will grab ADC Temp as well. On a Pico the port is 4
onBoardAdcTempPin = 4
tempSensorsPins =[
    # {"name": "Middle", "pin": 26},
    # {"name": "Top", "pin": 27},
    # {"name": "Outside", "pin": 28}
]
comboSensorsPins =[
    # {"name": "Entrance", "pin": 7}
     {"name": "Lid", "pin": 7}
]
 
hasScale = False
scaleDtPin= 9
scaleSckPin= 10

print("Hello, Pi Pico!")
print("BeeHiveCollector v 0.91")

pin_OUT = Pin(scaleDtPin, Pin.IN, pull=Pin.PULL_DOWN)
pin_SCK = Pin(scaleSckPin, Pin.OUT)

hx = HX711(pin_SCK, pin_OUT)


print("Hello, Pi Pico!")

class ComboSensor:
  def __init__(self, name, sensor: DHT22 ):
    self.name = name
    self.sensor = sensor

class D20Sensor:
  def __init__(self, name, sensor: DS18X20,roms ):
    self.name = name
    self.sensor = sensor
    self.roms = roms

def getD20Sensor(name, pin: int):
  try:
    print("Setting up D20 Pin: {} ".format(pin))
    p = Pin(pin)
    print("Starting Scan")
    ds = DS18X20(OneWire(Pin(pin)))
    r = ds.scan()[0]  # the one and only sensor
    print("Scan complete: {} ".format(r))
    return D20Sensor(name,ds,r)
  except Exception as e:
    print(e) 

def getComboSensor(name, pin: int):
  print("Setting up Combo Pin: {} ".format(pin))
  p = Pin(pin)
  s = dht.DHT22(p)
  return ComboSensor(name,s)
adc = {}
hx711 = {}


wlan = network.WLAN(network.STA_IF)

wlan.active(True)
def connectWifi():
  if wlan.isconnected():
      print("Already connected to Wifi")
      wdt.feed() #resets countdown
      return 1;

  print("Connecting to Wifi")
  wdt.feed() #resets countdown
  wlan.connect(wifiName, wifiPassword)

  # Wait for connection to establish
  max_wait = 10
  while max_wait > 0:
      if wlan.status() < 0 or wlan.status() >= 3:
              break
      max_wait -= 1
      wdt.feed() #resets countdown
      print('waiting for connection...')
      time.sleep(1)
      
  # Manage connection errors
  if wlan.status() != 3:
      return 0;
  else:
      print('connected')
      return 1;

while(connectWifi() == 0):
  print("Error connecting to Wifi")
  wdt.feed() #resets countdown
  sleep(5)

#if needed, overwrite default time server
ntptime.host = "1.europe.pool.ntp.org"

try:
  print("Local time before synchronization：%s" %str(time.localtime()))
  #make sure to have internet connection
  wdt.feed() #resets countdown
  ntptime.settime()
  wdt.feed() #resets countdown
  print("Local time after synchronization：%s" %str(time.localtime()))
except:
  print("Error syncing time")      

def connectMQTT():
  client = MQTTClient(client_id=bytes(beeHiveName,'utf-8'),
    server=bytes(mqqtServer,'utf-8'),
    port=mqqtPort,
    keepalive=7200,
    ssl=False,
  )

  print("Connecting to MQTT");
  client.connect()
  print("Connected to MQTT");
  return client 
   
wdt.feed() #resets countdown
client = connectMQTT();
wdt.feed() #resets countdown

def publish(topic, value):
  client.publish(topic, value)
  wdt.feed()
  print("publish Done")
submittingDataFailCount = 0;
wifiFailCount = 0
tempSensors = []
comboSensors = []
isSleeping = False        
          

def ReadAndSubmitData():
  global isSleeping
  global wifiFailCount
  while(connectWifi() == 0):
    print("Error connecting to Wifi")
    wifiFailCount = wifiFailCount + 1
    if(wifiFailCount > 10):
      machine.reset();
  wifiFailCount = 0
  client.reconnect()
  hasError = False
  timeRemaining = timeBetweenReadings
  try:
    pin.toggle()
    wdt.feed()
    sensorValues = [];
    print("Starting temp Sensors: {}".format(tempSensors.count));
    for sensor in tempSensors:
      try:
        wdt.feed()
        print("convert_temp");
        sensor.sensor.convert_temp()
        sleep(1)         # wait for results
        print("read_temp");
        wdt.feed()
        temp = sensor.sensor.read_temp(sensor.roms);
        print("sensorValues.append");
        wdt.feed()
        sensorValues.append({"name" : sensor.name, "type": "Temp", "value": temp});
        print("{} Temperature: {}°C  ".format(sensor.name, temp)) 
        wdt.feed()
      except Exception as e:
        print(e)

    print("Starting combo Sensors");
    for sensor in comboSensors:

      try:
        wdt.feed()
        print("measure");
        sensor.sensor.measure()
        wdt.feed()
        print("temperature");
        temp = sensor.sensor.temperature()
        print("humidity");
        hum = sensor.sensor.humidity()
        sensorValues.append({"name" : sensor.name, "type": "Temp", "value": temp});
        sensorValues.append({"name" : sensor.name, "type": "Humidity", "value": hum});
        wdt.feed()
        print("{} Temperature: {}°C   Humidity: {:.0f}% ".format(sensor.name, temp, hum))
      except Exception as e:
        print(e)
    if onBoardAdcTempPin > 0:
        try:
          wdt.feed()
          print("Onboard Temp Read");
          ADC_voltage = adc.read_u16() * (3.3 / (65535))
          temp = 27 - (ADC_voltage - 0.706)/0.001721
          temp_fahrenheit=32+(1.8*temp)
          sensorValues.append({"name" : "OnBoard", "type": "CPU_Temp", "value": temp});
          print("OnBoard Temperature: {}°C  ".format(temp))
          wdt.feed()
        except Exception as e:
          print(e)

    if(hasScale and hx711.is_ready()):
      try:
        wdt.feed()
        print("Starting Scale");
        units = hx711.get_value(10)
        sensorValues.append({"name" : "Scale", "type": "Weight", "value": units});
        print("Weight: {}g".format(units))
        wdt.feed()
      except Exception as e:
        print(e)
    snapShot = [{
        "time": time.time(),
        "data": {
            "hiveName": beeHiveName,
            "sensors": sensorValues
        }
    }]
    print("We have data, sending request")
    wdt.feed()
    j = json.dumps(snapShot)
    print(j)
    wdt.feed()
    
    for s in sensorValues:
        publish("{}/{}/{}/".format(beeHiveName, s["name"], s["type"]),str(s["value"]))
        wdt.feed()

    # r = urequests.post("https://api.axiom.co/v1/datasets/{}/ingest".format(dataset), timeout=30, data=j, headers={"Authorization": "Bearer {}".format(authToken), "Content-Type": "application/json"})
    # print(r.text)
    # r.close()
    
    timeRemaining = 60 - time.localtime()[5]
    print(timeRemaining)
    wdt.feed()
  except Exception as e:
    hasError = True
    print(e)
  if(hasError != True):
    isSleeping = True;
    pin.toggle()
    print("Sleeping for {} seconds".format(timeRemaining))  
    sleep(timeRemaining)
  else:
    isSleeping = True;
    sleep(5)
def second_thread():
  print("Hello, Data collection started!")
  sleep(5)

  if(hasScale):
    hx711 =  HX711(scaleDtPin,scaleSckPin);
    # hx711 set_scale(1);
    hx711.tare();
  try:
    print("Setting up Sensors")
    if onBoardAdcTempPin > 0:
        adc = ADC(onBoardAdcTempPin)
    for sensor in tempSensorsPins:
      tempSensors.append(getD20Sensor(sensor["name"], sensor["pin"]))
    for sensor in comboSensorsPins:
      comboSensors.append(getComboSensor(sensor["name"], sensor["pin"]))
  except Exception as e:
      print(e)

  global isSleeping
  global submittingDataFailCount
  while True:
    try:

      isSleeping = False
      ReadAndSubmitData()
      isSleeping = True
      submittingDataFailCount = 0;
    except Exception as e:
      submittingDataFailCount = submittingDataFailCount + 1
      print(e)
      print("Error reading data " + str(submittingDataFailCount) + " times")

    
_thread.start_new_thread(second_thread, ())


while True:
  pin.toggle()
  utime.sleep(0.25)
  if(isSleeping == True):
    # print("sleeping: So I feed")
    wdt.feed() #resets countdown
  # else:
  #   print("Not sleeping: So I don't feed")
  if(submittingDataFailCount > 5):
    machine.reset();