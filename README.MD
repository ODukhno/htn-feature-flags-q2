# Feature Flighting on Continuous Deployment
The project is an internal hackaton project. The quake2 was selected as a playground
since this is fun to work on it and then demo it.

# Description TBD

# Running the project 

## Depenencies to run locally
1) install docker - for containerization
2) install kubectl - for k8s deployment
3) install minicube - serves as a k8s cluster
4) do not forget to update client secret in the k8s configs
5) you should generate Docker private registry secret (name:acrsecret) as described [there](https://minikube.sigs.k8s.io/docs/handbook/registry/#docker-on-windows)

## Just run ./Deploy.ps1 from the root. The script will: 
- build all .net projects 
- build docker containers
- publish docker containers
- deploy apps in k8s cluster