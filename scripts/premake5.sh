#!/bin/bash
##########################################################"
# Copyright © 2022 by Renaud Guillard (dev@nore.fr)
# Distributed under the terms of the MIT License, see LICENSE
##########################################################"
 
cd "$(dirname "${0}")"
for action in vs2019
do
	premake5 --dotnet=mono  ${action}
done
