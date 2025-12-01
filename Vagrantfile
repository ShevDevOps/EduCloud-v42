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
	ubuntu.vm.provider "virtualbox" do |vb|
      # Встановлюємо 4GB RAM
      vb.memory = "4096"
      # Рекомендовано також додати 2 CPU
      vb.cpus = 2
    end

    ubuntu.vm.provision "shell", privileged: false, inline: <<-SHELL
		echo "--- Встановлення .NET 8 SDK на Ubuntu ---"
		wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
		chmod +x ./dotnet-install.sh

		./dotnet-install.sh --channel 8.0 

		echo 'export DOTNET_ROOT=/home/vagrant/.dotnet' >> /home/vagrant/.bashrc
		echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> /home/vagrant/.bashrc
		export DOTNET_ROOT=/home/vagrant/.dotnet
		export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

		MSSQL_PASSWORD="1Qwerty." 
		PG_PASSWORD="qwerty"


		echo "--- Updating packages and installing dependencies ---"
		sudo apt-get update
		sudo apt-get install -y curl gpg ca-certificates lsb-release

		# ==========================================
		# 1. Налаштування репозиторіїв MS SQL (Адаптовано для 20.04)
		# ==========================================
		echo "--- Setting up MS SQL Server repository for Ubuntu 20.04 (Focal) ---"

		curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | sudo gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
		sudo chmod a+r /usr/share/keyrings/microsoft-prod.gpg

		# === ЗМІНЕНО ===
		# Використовуємо репозиторії 20.04 / focal
		echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/20.04/mssql-server-2022 focal main" | sudo tee /etc/apt/sources.list.d/mssql-server-2022.list
		echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/ubuntu/20.04/prod focal main" | sudo tee /etc/apt/sources.list.d/msprod.list
		# === /ЗМІНЕНО ===

		sudo apt-get update

		# ==========================================
		# 2. Встановлення MS SQL Server
		# ==========================================
		echo "--- Installing MS SQL Server ---"
		# На Ubuntu 20.04 libssl1.1 є рідною, тому хаки не потрібні
		sudo apt-get install -y mssql-server

		echo "--- Configuring MS SQL Server ---"
		sudo MSSQL_SA_PASSWORD=$MSSQL_PASSWORD \
			 MSSQL_PID='Developer' \
			 /opt/mssql/bin/mssql-conf -n setup accept-eula

		echo "--- Checking MS SQL Server status ---"
		sleep 5
		systemctl status mssql-server --no-pager

		# ==========================================
		# 3. Встановлення інструментів MS SQL
		# ==========================================
		echo "--- Installing MS SQL Tools ---"
		sudo ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev
		echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc

		# ==========================================
		# 4. Встановлення PostgreSQL
		# ==========================================
		echo "--- Installing PostgreSQL ---"
		sudo apt-get install -y postgresql postgresql-contrib

		echo "--- Configuring PostgreSQL Password ---"
		sudo -u postgres psql -c "ALTER USER postgres PASSWORD '$PG_PASSWORD';"

		echo "--- Installation Complete! ---"
	  
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

		# Перевірка та встановлення Chocolatey
		if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
			Write-Host "Installing Chocolatey..."
			Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
		}

		$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

		# ==========================================
		# Встановлення MS-SQL Server (Mixed Mode)
		# ==========================================

		# ЗАДАЙТЕ ПАРОЛЬ ТУТ (Має бути складним: букви, цифри, спецсимволи!)
		$mssqlPassword = "1Qwerty."

		Write-Host "Installing MS-SQL Server 2022 with Mixed Mode Auth..."

		# Додані параметри:
		# /SECURITYMODE=SQL — вмикає змішану аутентифікацію
		# /SAPWD — встановлює пароль для користувача 'sa'
		choco install sql-server-2022 -y --params "'/IAcceptSQLServerLicenseTerms /SECURITYMODE=SQL /SAPWD=`"$mssqlPassword`"'"

		# ==========================================
		# Встановлення PostgreSQL
		# ==========================================
		$pgPassword = "qwerty"

		Write-Host "Installing PostgreSQL..."
		choco install postgresql -y --params "/Password:$pgPassword"

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