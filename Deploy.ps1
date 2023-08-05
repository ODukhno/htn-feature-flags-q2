param(
    $Version = "7",
    $ACRName = "q2ffhackaton",
    $SubId = "d914f7f6-9bd1-40a8-ad23-68ae62cf2394",
    $ResourceGroup = "q2-dt-hackaton",
    $Location = "West US 3",
    $DeploymentName = "q2ffhackaton"
)

function BuildAndPublshContainerImages {
    param (
        [string]$ContainerName,
        [string]$SubDir
    )
    
    cd "$PSScriptRoot\$SubDir"
    
    docker build -t $ContainerName . 

    $Image = $ACRName+".azurecr.io/"+$ContainerName+":"+$Version

    docker tag docker.io/library/$ContainerName $Image
    docker push $Image
    
    Write-Host "Published " $Image " " $Version
    
    $Image = $ACRName+".azurecr.io/"+$ContainerName

    docker tag docker.io/library/$ContainerName $Image
    docker push $Image

    Write-Host "Published " $Image " latest"
}

Connect-AzAccount
Select-AzSubscription -SubscriptionId $SubId
Connect-AzContainerRegistry -Name $ACRName

BuildAndPublshContainerImages q-resources "QResourceserver"
BuildAndPublshContainerImages q-entities "QEntitiesServer"
BuildAndPublshContainerImages ecs-config-proxy "ECSConfigProxy"

cd "$PSScriptRoot"

cd "$PSScriptRoot/k8s"

kubectl apply -f q-resources.yaml
kubectl apply -f q-entities.yaml
kubectl apply -f flagd.yaml
kubectl apply -f ecs-config-proxy.yaml

Write-Host "At this moment you should tunnel q-resources and q-entities service with minicube. E.g. minikube.exe service q-resources --url"