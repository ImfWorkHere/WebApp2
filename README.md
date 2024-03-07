## About 
A RESTful API for searching geo names (cities, rivers and so on). Built using ASP.Net, Entity Framework, SQL Server, Seq, Docker, docker-compose. Taking data from [here](http://geonames.org).

[!NOTE] Free data dump from geoname.org is no longer available.

## Documentation
API supports following methods:

### GET `(host)/api/geonames(?Query params)`
| Query params | Data type |
| - | - |
| Name | string |
| Latitude | double |
| Longitude | double|
| Radius | double |
| MinimumPopulation | uint |
| MaximumPopulation | uint |
| Count | int |

Return 
```
[
	{
        "id": int,
        "name": "",
        "asciiName": "",
        "alternativeNames": "",
        "latitude": double,
        "longitude": double,
        "featureClass": "",
        "featureCode": "",
        "countryCode": "",
        "countryCode2": "",
        "adminCode1": "",
        "adminCode2": "",
        "adminCode3": "",
        "adminCode4": "",
        "population": int,
        "elevation": int?,
        "dem": int,
        "timeZone": "",
        "modificationDate": ""
    },
    {...}
]
```
*Can also return status code 404 Not Found!*

#### Example
Request:
`(host)/api/geonames/get?Name=Minsk&Latitude=54&Longitude=28&Radius=100`

Response:
```
[
    {
        "id": 625144,
        "name": "Minsk",
        "asciiName": "Minsk",
        "alternativeNames": "MSQ,Mins'k,Minsc,Minscum,Minsk,Minsk - Minsk,Minsk,Minsk osh,Minska,Minskaj,Minskas,Minsko,Minszk,Minsk,Myensk,Myenyesk,Mînsk,ming si ke,ming si ke shi,minseukeu,minsk,minsuku,mnsk,mynsk,mynsq,mynysky",
        "latitude": 53.9,
        "longitude": 27.56667,
        "featureClass": "P",
        "featureCode": "PPLC",
        "countryCode": "BY",
        "countryCode2": "",
        "adminCode1": "04",
        "adminCode2": "",
        "adminCode3": "",
        "adminCode4": "",
        "population": 1742124,
        "elevation": null,
        "dem": 222,
        "timeZone": "Europe/Minsk",
        "modificationDate": "2022-05-05T00:00:00"
    }
]
```
