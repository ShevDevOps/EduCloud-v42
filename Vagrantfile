# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|

  # --- VM 1: UBUNTU ---
  config.vm.define "ubuntu_vm" do |ubuntu|
    ubuntu.vm.box = "ubuntu/focal64"
    ubuntu.vm.boot_timeout = 1200

    ubuntu.vm.hostname = "ubuntu-server"
    ubuntu.vm.network "private_network", ip: "192.168.33.10"
    
    ubuntu.vm.network "forwarded_port", guest: 5000, host: 5000

    ubuntu.vm.provision "shell", privileged: false, inline: <<-SHELL
      echo "--- Встановлення .NET 8 SDK на Ubuntu ---"
      wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
      chmod +x ./dotnet-install.sh
      
      ./dotnet-install.sh --channel 8.0 
      
      echo 'export DOTNET_ROOT=/home/vagrant/.dotnet' >> /home/vagrant/.bashrc
      echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> /home/vagrant/.bashrc
      export DOTNET_ROOT=/home/vagrant/.dotnet
      export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
      
      echo "--- Налаштування приватного BaGet репозиторію ---"
      /home/vagrant/.dotnet/dotnet nuget add source "http://192.168.33.1:5555/v3/index.json" -n "MyBaGet"

      echo "--- Встановлення EduCloud-v42 з BaGet ---"
      /home/vagrant/.dotnet/dotnet tool install --global EduCloud-v42 --version 1.0.3-lab2
      
      echo "--- Встановлення завершено! ---"
    SHELL
  end

  # --- VM 2: WINDOWS ---
  config.vm.define "windows_vm" do |windows|
    windows.vm.box = "stromweld/windows-2019" 
    windows.vm.boot_timeout = 1200
    windows.vm.hostname = "windows-server"
    windows.vm.guest = :windows
    windows.vm.communicator = "winrm"
    windows.vm.network "private_network", ip: "192.168.33.11"
    
    windows.vm.network "forwarded_port", guest: 5000, host: 5001

    windows.vm.provision "shell", inline: <<-PSHELL
      Write-Host "--- Installing .NET 8 SDK on Windows ---"
      Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile .\\dotnet-install.ps1
      .\\dotnet-install.ps1 -Channel 8.0

      # 1. Визначаємо шляхи
	  $dotnetPath = "$env:USERPROFILE\\AppData\\Local\\Microsoft\\dotnet"
      $dotnetToolsPath = "$env:USERPROFILE\\.dotnet\\tools"

      # 2. Встановлюємо PATH для ПОТОЧНОЇ сесії
      $env:PATH = "$env:PATH;$dotnetPath;$dotnetToolsPath"

      # 3. Встановлюємо PATH НАЗАВЖДИ для користувача 'vagrant'
      $oldPath = [System.Environment]::GetEnvironmentVariable('PATH', 'Machine')
      # Перевіряємо, чи шлях вже не додано
      if ($oldPath -notlike "*$dotnetPath*") {
          $newPath = "$oldPath;$dotnetPath;$dotnetToolsPath"
          [System.Environment]::SetEnvironmentVariable('PATH', $newPath, 'Machine')
          Write-Host "Dotnet PATH added to user environment."
      } else {
          Write-Host "Dotnet PATH already exists in user environment."
      }
	  

      Write-Host "--- Configuring private BaGet repository ---"
      dotnet nuget add source "http://192.168.33.1:5555/v3/index.json" -n "MyBaGet"

      Write-Host "--- Installing EduCloud-v42 from BaGet ---"
      dotnet tool install --global EduCloud-v42 --version 1.0.3-lab2

	  cd $env:USERPROFILE\\.dotnet\\tools
	  echo "Set-NetFirewallProfile -Profile Domain, Public, Private -Enabled False`
	  `$env:ASPNETCORE_URLS = 'http://0.0.0.0:5000'`
	  `$env:PATH = [System.Environment]::GetEnvironmentVariable('Path','Machine')`
	  cd $env:USERPROFILE\\.dotnet\\tools\\.store\\educloud-v42\\1.0.3-lab2\\educloud-v42\\1.0.3-lab2\\tools\\net8.0\\any`
	  dotnet EduCloud-v42.dll" > educloudMain.ps1
	  
	  Set-Content -Encoding utf8 "powershell.exe -File C:\\Users\\vagrant\\.dotnet\\tools\\educloudMain.ps1" -Path educloud.cmd

      Write-Host "--- Installing complete! ---"
    PSHELL
  end
end