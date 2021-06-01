
Ctrl-C
After tool started you can use Ctrl-C for close application.

ESC
In archive or restore process you can use ESC for safe application exit.
It is wait until the current process finishes.

Switches:
-y, --system ... Set the system to be archived
-k, --skip [optional] ... Skip SAP check
-i, --invert [optional] ... Archive / restore tables that are NOT in CSV
-a, --archive [optional] ... Archive tables that are in CSV
-r, --restore [optional] ... Restore tables that are in CSV
-e, --export [optional] ... Export table names
-x, --fullexport [optional] ... Export tables with additional informations
-l, --list [optional] ... List all tables to console
-s, --server [optional] ... Set target MySQL server. Default: localhost
-u, --user [optional] ... Set username to connection. Default: root
-p, --password [optional] ... Set password. Default: avendata
-d, --database [optional] ... Set Viewbox database. Default: viewbox
-f, --file [optional] ... Set import or export CSV file path. Default: tables.csv
-m, --port [optional] ... Set the port of the server
