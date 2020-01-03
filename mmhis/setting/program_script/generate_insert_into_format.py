#this program generates the sql statements to populate the format table

f = open (r'C:\workAHTD\mmhis_authoring\mmhis\mmhis_data_2\mmhis_data_2_vs2012\query\generate_insert_into_format_input.txt')
content = list(map(lambda x: x.rstrip(), f.readlines()))
f.close()

f = open (r'C:\workAHTD\mmhis_authoring\mmhis\mmhis_data_2\mmhis_data_2_vs2012\query\generate_insert_into_format_output.txt', 'w')

import re
first_line = content[0]
for i in range(1, len(content)):
	line = re.split(', ', content[i].lstrip())
	print (first_line + "'" + line[0] + "', '" + line[1] + "', '" + line[2]  + "', " + line[3] + ", '" + line[4] + "', " + line[5] + ", '" + line[6] + "', " + line[7] + ", '" + line[8] + "', '" + line[9] + "', " + line[10] + ", '" + line[11] + "');", end = '\n\n', file = f);
f.close()
