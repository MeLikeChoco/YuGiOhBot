FROM mcr.microsoft.com/dotnet/sdk:6.0.401

WORKDIR src/

COPY wait-for-it.sh ./

COPY *.sln ./

#COPY YuGiOh.Database/YuGiOh.Database.csproj ./YuGiOh.Database/
#COPY Discord.Addons.Interactive/Discord.Addons.Interactive.csproj ./Discord.Addons.Interactive/
#COPY YuGiOh.Common/YuGiOh.Common.csproj ./YuGiOh.Common/
#COPY YuGiOh.Scraper/YuGiOh.Scraper.csproj ./YuGiOh.Scraper/
#COPY YuGiOh.Bot/YuGiOh.Bot.csproj ./YuGiOh.Bot/
#COPY YuGiOh.Common.Test/YuGiOh.Common.Test.csproj ./YuGiOh.Common.Test/
#COPY YuGiOh.Bot.Test/YuGiOh.Bot.Test.csproj ./YuGiOh.Bot.Test/
#
#RUN dotnet restore

##YES YES I KNOW, lack of caching csproj files, but I'm way too lazy to maintain them
##If I add a new project or delete a project, I have to REMEMBER to update Dockerfile
##I have no trust in myself remembering 
COPY . .

RUN chmod +x wait-for-it.sh

ENTRYPOINT ["./wait-for-it.sh", "db:5432", "--strict", "--timeout=60", "--", "dotnet", "test", "--verbosity=normal"]
#ENTRYPOINT dotnet test --verbosity normal