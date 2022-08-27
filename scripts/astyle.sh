#!/bin/bash
##########################################################"
# Copyright © 2022 by Renaud Guillard (dev@nore.fr)
# Distributed under the terms of the MIT License, see LICENSE
##########################################################"
 
cd "$(dirname "${0}")/.." \
&& find src -name '*.cs' \
	| xargs astyle --options=scripts/astyle.style \
	1>/dev/null 
