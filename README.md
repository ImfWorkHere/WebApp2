# WebApp2

## Beginning
This is my first completed solution. It was created to copy data from geonames.org to SQL Server and to have access to this data. Here I split my app in 4 folders (4 projects):

- Domain - Just models and no logic
- Application - Abstractions
- Infrastructure - Realizations and some background logic
- Web UI - Controller that provides access to db

The thing I like here is that I can really hide realizations, and have no chance to use them instead of  abstractions and I can publish Domain project to NuGet to use this models later in another solution.

## Db
At the beginning I tried to use code-first db, but then realized that I can just copy T-SQL and don't waste some folders and memory on EF Tools (EF design NuGet package) and migrations. Also thought about where to store connection strings. User secrets is a good place for debugging, but on release with docker I tried to use environment variables and still use them (Azure key storage or Google/Amazon alternatives also looks pretty good). 

## Memory
Once I found, that I use quite a lot of CPU memory, and this value was only increasing. Then I found a problem with my repository, that just copies all db and then select things. It was also connected with my will of getting cities in radius of some km, and EF have no adapter to this kind of method (or I just haven't found it yet). So I changed this thing and also added call of GC at 4-5 am. 

## Asynchronous methods
In this solution I had a background service, that copy data to my SQL db. But at one moment I found that my controller didn't work. Then I realized that my web UI don't even hosted. The problem was in this background service. It has to work asynchronously, but I was awaiting him. That's kinda funny because he also was waiting till next day to update db.


