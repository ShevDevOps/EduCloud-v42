# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|

  # --- VM 1: UBUNTU ---
  config.vm.define "ubuntu_vm" do |ubuntu|
    ubuntu.vm.box = "ubuntu/bionic64" # Ubuntu 18.04
    ubuntu.vm.hostname = "ubuntu-server"
    ubuntu.vm.network "private_network", ip: "192.168.33.10"

    ubuntu.vm.provision "shell", inline: <<-SHELL
      echo "--- Встановлення .NET 6 SDK на Ubuntu ---"
      wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
      chmod +x ./dotnet-install.sh
      ./dotnet-install.sh --channel 6.0
      export PATH=$PATH:/home/vagrant/.dotnet:/home/vagrant/.dotnet/tools
      
      echo "--- Налаштування приватного BaGet репозиторію ---"
      /home/vagrant/.dotnet/dotnet nuget add source "http://192.168.33.1:5555/v3/index.json" -n "MyBaGet"

      echo "--- Встановлення EduCloud-v42 з BaGet ---"
      # Встановлюємо інструмент
      /home/vagrant/.dotnet/dotnet tool install --global EduCloud-v42 --version 1.0.0-lab2
      
      echo "--- Встановлення завершено! ---"
    SHELL
  end

  # --- VM 2: WINDOWS ---
  config.vm.define "windows_vm" do |windows|
    windows.vm.box = "generic/windows2019"
    windows.vm.hostname = "windows-server"
    windows.vm.guest = :windows
    windows.vm.communicator = "winrm"
    windows.vm.network "private_network", ip: "192.168.33.11"
    
    windows.vm.provision "shell", inline: <<-PSHELL
      Write-Host "--- Встановлення .NET 6 SDK на Windows ---"
      Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile .\\dotnet-install.ps1
      .\\dotnet-install.ps1 -Channel 6.0
      $env:PATH = "$env:PATH;$env:USERPROFILE\\.dotnet;$env:USERPROFILE\\.dotnet\\tools"

      Write-Host "--- Налаштування приватного BaGet репозиторію ---"
      dotnet nuget add source "http://192.168.33.1:5555/v3/index.json" -n "MyBaGet"
      
      Write-Host "--- Встановлення EduCloud-v42 з BaGet ---"
      # Встановлюємо ваш інструмент
      dotnet tool install --global EduCloud-v42 --version 1.0.0-lab2
      
      Write-Host "--- Встановлення завершено! ---"
    PSHELL
  end
end