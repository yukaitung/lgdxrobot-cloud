#  LGDXRobot Cloud Robot Management System 

> Please note that development is primarily done on GitLab: https://gitlab.com/yukaitung/lgdxrobot2-cloud

LGDXRobot Cloud is a cloud-based robot management system for LGDXRobot, designed with a focus on flexibility and security. It can monitor the status of your robots and control navigation tasks using customised workflows.

- [Homepage](https://lgdxrobot.bristolgram.uk/cloud/)
- [Documentation](https://docs.lgdxrobot.bristolgram.uk/cloud/)

## Notes

Use the following command to generate the client in LGDXRobotCloud.UI:

```bash
kiota generate -l CSharp -c LgdxApiClient -n LGDXRobotCloud.UI.Client -o ./Client -d https://localhost:5163/openapi/v1.json --clean-output
```

Use the following command to build the Docker image:

```bash
docker build -t lgdxrobotcloud.api -f Dockerfile.api .
docker build -t lgdxrobotcloud.ui -f Dockerfile.ui .
docker build -t lgdxrobotcloud.worker -f Dockerfile.worker .
```

```bash
docker buildx build --platform linux/amd64,linux/arm64 -t lgdxrobotcloud.api -f Dockerfile.api .
```
