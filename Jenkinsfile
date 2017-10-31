try {
	node('Windows') {
		stage 'Checkout'
			deleteDir()
			checkout scm

		stage 'Build'
			
			bat '''
				nuget restore .

				dotnet publish .\\Abiomed.DotNetCore.FactoryData.Loader\\Abiomed.DotNetCore.FactoryData.Loader.csproj -c Release /p:PublishDir="%WORKSPACE%\\FactoryData"
				7z a FactoryData_v01_%BRANCH_NAME%_%BUILD_NUMBER%.zip FactoryData/

				dotnet publish .\\Abiomed.DotNetCore.MailQueueService\\Abiomed.DotNetCore.MailQueueService.csproj -c Release /p:PublishDir="%WORKSPACE%\\MailQueueService"
				7z a MailQueueService_v01_%BRANCH_NAME%_%BUILD_NUMBER%.zip MailQueueService/

				dotnet publish .\\Abiomed.DotNetCore.MessagePump\\Abiomed.DotNetCore.MessagePump.csproj -c Release /p:PublishDir="%WORKSPACE%\\MessagePump"
				7z a MessagePump_v01_%BRANCH_NAME%_%BUILD_NUMBER%.zip MessagePump/

				dotnet publish .\\Abiomed.DotNetCore.OCRService\\Abiomed.DotNetCore.OCRService.csproj -c Release /p:PublishDir="%WORKSPACE%\\OCRService"
				7z a OCRService_v01_%BRANCH_NAME%_%BUILD_NUMBER%.zip OCRService/

				dotnet publish .\\Abiomed.RLR\\Abiomed.RLR.csproj -c Release /p:PublishDir="%WORKSPACE%\\RLR"
				7z a RLR_v01_%BRANCH_NAME%_%BUILD_NUMBER%.zip RLR/
			'''

		stage 'Archive'
			archiveArtifacts '*.zip'

	}
	mail ( 
        to: 'ppamidi@abiomed.com,aagnello@abiomed.com,plemay@abiomed.com,rlussier@abiomed.com,msami@abiomed.com,rweigand@abiomed.com,glim@abiomed.com,rsrinivasan@abiomed.com',
        subject: "RemoteLinkCloud ${BRANCH_NAME} -  Build Notification - SUCCESS - New Code Pushed to Git", 
        body: """RemoteLinkCloud ${BRANCH_NAME} - SUCCESS
	Download the artifacts from the below link:
	http://rlmbuild.abiomed.com/job/RLCloudMultibranchPipeline/job/${BRANCH_NAME}/${BUILD_NUMBER}/artifact/"""
    
    )
} catch(e) {
    echo e.message
    mail( to: 'ppamidi@abiomed.com,aagnello@abiomed.com,plemay@abiomed.com,rlussier@abiomed.com,msami@abiomed.com,rweigand@abiomed.com,glim@abiomed.com,rsrinivasan@abiomed.com',
          subject: 'RemoteLinkCloud ${BRANCH_NAME} -  Build Notification - FAILED - New Code Pushed to Git', 
          body: "See buildlog at ${env.BUILD_URL}/console")
    error e.message
}
