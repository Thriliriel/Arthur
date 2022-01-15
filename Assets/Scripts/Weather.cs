using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Weather : MonoBehaviour
{
	public LocationInfo Info;
	public float latitude;
	public float longitude;
	private string IPAddress;
	public MainController mc;

	public void WeatherNow(bool isToday = true)
    {
		StartCoroutine(GetIP(isToday));
    }

	private IEnumerator GetIP(bool isToday)
	{
		var www = new UnityWebRequest("https://geo.ipify.org/api/v2/country?apiKey=at_RI3PCsSVz80L3VIKVjn6esSTsh9kn")
		{
			downloadHandler = new DownloadHandlerBuffer()
		};

		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			//error
			yield break;
		}

		IPAddress = www.downloadHandler.text;
		Debug.Log("Ret: " + IPAddress);
		string[] splito = IPAddress.Split(',');
		splito[0] = splito[0].Replace("\\", "");
		splito[0] = splito[0].Replace("\"", "");
		splito[0] = splito[0].Replace("ip:", "%");
		splito = splito[0].Split('%');

		IPAddress = splito[1];

		Debug.Log("IP: " + IPAddress);
		StartCoroutine(GetCoordinates(isToday));
	}

	private IEnumerator GetCoordinates(bool isToday)
	{
		var www = new UnityWebRequest("http://ip-api.com/json/" + IPAddress)
		{
			downloadHandler = new DownloadHandlerBuffer()
		};

		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			//error
			yield break;
		}

		Info = JsonUtility.FromJson<LocationInfo>(www.downloadHandler.text);
		latitude = Info.lat;
		longitude = Info.lon;

		Debug.Log("Location: " + latitude + " - " + longitude);

		//if it is today, call the weather. Otherwise, get for tomorrow.
		if(isToday)
			StartCoroutine(GetWeather());
		else
			StartCoroutine(GetWeatherTomorrow());
	}

	private IEnumerator GetWeather()
    {
		var www = new UnityWebRequest("http://api.weatherapi.com/v1/current.json?key=3937ec4a595a49da8f6180715221301&q=" + latitude + "," + longitude)
		{
			downloadHandler = new DownloadHandlerBuffer()
		};

		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			//error
			yield break;
		}

		/*Weather: {"location":{"name":"Passo Fundo","region":"Rio Grande do Sul","country":"Brazil","lat":-28.29,"lon":-52.44,
			* "tz_id":"America/Sao_Paulo","localtime_epoch":1642097673,"localtime":"2022-01-13 15:14"},
			* "current":{"last_updated_epoch":1642093200,"last_updated":"2022-01-13 14:00","temp_c":37.8,"temp_f":100.0,
			* "is_day":1,"condition":{"text":"Sunny","icon":"//cdn.weatherapi.com/weather/64x64/day/113.png","code":1000},
			* "wind_mph":5.6,"wind_kph":9.0,"wind_degree":50,"wind_dir":"NE","pressure_mb":1013.0,"pressure_in":29.91,
			* "precip_mm":0.0,"precip_in":0.0,"humidity":16,"cloud":4,"feelslike_c":36.8,"feelslike_f":98.2,"vis_km":10.0,
			* "vis_miles":6.0,"uv":9.0,"gust_mph":6.5,"gust_kph":10.4}}*/
		string stuff = www.downloadHandler.text.Replace("\\", "");
		stuff = stuff.Replace("\"", "");
		stuff = stuff.Replace("condition:{text:", "%");
		stuff = stuff.Replace("feelslike_c:", "#");

		//condition
		string[] cond = stuff.Split('%');
		cond = cond[1].Split(',');

		//temperature
		string[] temper = stuff.Split('#');
		temper = temper[1].Split(',');

		Debug.Log("Weather: " + cond[0]);
		Debug.Log("Temperature: " + temper[0]);

		//speak it!
		mc.SpeakYouFool("The weather in your location now is: " + cond[0] + ". The temperature is " + temper[0] + " degrees.");

		/*Info = JsonUtility.FromJson<WeatherInfo>(www.downloadHandler.text);
		currentWeatherText.text = "Current weather: " + Info.currently.summary;*/
	}

	private IEnumerator GetWeatherTomorrow()
	{
		var www = new UnityWebRequest("http://api.weatherapi.com/v1/forecast.json?key=3937ec4a595a49da8f6180715221301&q=" + latitude + "," + longitude+"&days=2")
		{
			downloadHandler = new DownloadHandlerBuffer()
		};

		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			//error
			yield break;
		}

		DateTime today = DateTime.Today;
		DateTime tomorrow = today.AddDays(1);
		string month = tomorrow.Month.ToString();
		string day = tomorrow.Day.ToString();

		if(tomorrow.Month < 10) month = "0"+tomorrow.Month.ToString();
		if (tomorrow.Day < 10) day = "0" + tomorrow.Day.ToString();

		string tomorrowDate = tomorrow.Year + "-" + month + "-" + day;

		/*date_epoch:1642291200,day:{maxtemp_c:36.7,maxtemp_f:98.1,mintemp_c:19.1,mintemp_f:66.4,avgtemp_c:26.6,avgtemp_f:79.9,
		 * maxwind_mph:15.2,maxwind_kph:24.5,totalprecip_mm:3.6,totalprecip_in:0.14,avgvis_km:9.8,avgvis_miles:6.0,
		 * avghumidity:62.0,daily_will_it_rain:1,daily_chance_of_rain:88,daily_will_it_snow:0,daily_chance_of_snow:0,
		 * condition:{text:Patchy rain possible,icon://cdn.weatherapi.com/weather/64x64/day/176.png,code:1063}*/
		//Debug.Log("AAAA: " + www.downloadHandler.text);
		string stuff = www.downloadHandler.text.Replace("\\", "");
		stuff = stuff.Replace("\"", "");
		stuff = stuff.Replace("date:"+ tomorrowDate, "$");
		string[] forec = stuff.Split('$');

		forec[1] = forec[1].Replace("condition:{text:", "%");
		forec[1] = forec[1].Replace("maxtemp_c:", "#");
		forec[1] = forec[1].Replace("mintemp_c:", "$");

		stuff = forec[1];

		//condition
		string[] cond = stuff.Split('%');
		cond = cond[1].Split(',');

		//temperature max
		string[] temperMax = stuff.Split('#');
		temperMax = temperMax[1].Split(',');

		//temperature min
		string[] temperMin = stuff.Split('$');
		temperMin = temperMin[1].Split(',');

		Debug.Log("Weather: " + cond[0]);
		Debug.Log("Temperature: " + temperMin[0] + " - " + temperMax[0]);

		//speak it!
		mc.SpeakYouFool("The weather in your location tomorrow will be: " + cond[0] + ". The temperature will vary between " +
			"" + temperMin[0] + " and " + temperMax[0] + " degrees.");

		/*Info = JsonUtility.FromJson<WeatherInfo>(www.downloadHandler.text);
		currentWeatherText.text = "Current weather: " + Info.currently.summary;*/
	}
}

	
[Serializable]
public class WeatherInfo
{
	public float latitude;
	public float longitude;
	public string timezone;
	public Currently currently;
	public int offset;
}

[Serializable]
public class Currently
{
	public int time;
	public string summary;
	public string icon;
	public int nearestStormDistance;
	public int nearestStormBearing;
	public int precipIntensity;
	public int precipProbability;
	public double temperature;
	public double apparentTemperature;
	public double dewPoint;
	public double humidity;
	public double pressure;
	public double windSpeed;
	public double windGust;
	public int windBearing;
	public int cloudCover;
	public int uvIndex;
	public double visibility;
	public double ozone;
}

[Serializable]
public class LocationInfo
{
	public string status;
	public string country;
	public string countryCode;
	public string region;
	public string regionName;
	public string city;
	public string zip;
	public float lat;
	public float lon;
	public string timezone;
	public string isp;
	public string org;
	public string @as;
	public string query;
}
