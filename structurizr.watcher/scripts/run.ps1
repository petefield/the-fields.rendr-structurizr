docker run --rm -v ${PWD}:/root/data ghcr.io/aidmax/structurizr-cli-docker:latest export -w /root/data/temp.dsl -f plantuml/c4plantuml
$text = Get-Content .\structurizr-SystemContext.puml -Raw 
Invoke-WebRequest -Uri http://localhost:8080/c4plantuml/png -Method POST -ContentType text/plain -Body $text -OutFile "temp.png"      