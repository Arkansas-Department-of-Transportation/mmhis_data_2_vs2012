#this program takes the output of ms access documentor as input and parse its
#content to generate the create index sql statements and saves the result to
#the specified output file

f = open (r'C:\workAHTD\mmhis_authoring\mmhis\mmhis_data_2\mmhis_data_2_vs2012\query\generate_create_index_input.txt')
content = list(map(lambda x: x.rstrip(), f.readlines()))
f.close()

f = open (r'C:\workAHTD\mmhis_authoring\mmhis\mmhis_data_2\mmhis_data_2_vs2012\query\generate_create_index_output.txt', 'w')

start_field_list = 0
import re
for i in range(len(content)):
	line = re.split('\t| ', content[i].lstrip())
	if line[0] == 'create' and line[1] == 'table':
		table_name = line[2]
		print (content[i], file = f)
	elif line[0] == 'Name:':
		index_name = line[2]
		print ('create index [' + index_name + '] on ' + table_name + ' ', end = '', file = f)
	elif line[0] == 'Fields:':
		start_field_list = 1
	elif start_field_list > 0:
		if len(content[i].lstrip()) == 0:
			if index_name == 'PrimaryKey':
				print (') with primary;', end = '\n\n', file = f)
			else:
				print (');', end = '\n\n', file = f)
			start_field_list = 0
		elif start_field_list == 1:
			print ('([' + line[0] + ']', end = '', file = f)
			start_field_list = 2
		else:
			print (', [' + line[0] + ']', end = '', file = f)
	else:
		print (content[i], file = f)
f.close()
