import os

replacements = {
    'namespace INVENTO_FLOW.Models': 'namespace InventoFlow.Domain.Entities',
    'using INVENTO_FLOW.Models;': 'using InventoFlow.Domain.Entities;',
    'namespace INVENTO_FLOW.DTOs': 'namespace InventoFlow.Application.DTOs',
    'using INVENTO_FLOW.DTOs': 'using InventoFlow.Application.DTOs',
    'namespace INVENTO_FLOW.Repositories.Interfaces': 'namespace InventoFlow.Application.Interfaces.Repositories',
    'using INVENTO_FLOW.Repositories.Interfaces;': 'using InventoFlow.Application.Interfaces.Repositories;',
    'namespace INVENTO_FLOW.Services.Interfaces': 'namespace InventoFlow.Application.Interfaces.Services',
    'using INVENTO_FLOW.Services.Interfaces;': 'using InventoFlow.Application.Interfaces.Services;',
    'namespace INVENTO_FLOW.Services': 'namespace InventoFlow.Application.Services',
    'using INVENTO_FLOW.Services;': 'using InventoFlow.Application.Services;',
    'namespace INVENTO_FLOW.Data': 'namespace InventoFlow.Infrastructure.Data',
    'using INVENTO_FLOW.Data;': 'using InventoFlow.Infrastructure.Data;',
    'namespace INVENTO_FLOW.Repositories': 'namespace InventoFlow.Infrastructure.Repositories',
    'using INVENTO_FLOW.Repositories;': 'using InventoFlow.Infrastructure.Repositories;'
}

base_dir = r'c:\Users\SAO BIEN\source\repos\INVENTO-FLOW'

for root, dirs, files in os.walk(base_dir):
    # Exclude obj and bin dirs
    if 'obj' in root or 'bin' in root or '.git' in root or '.vs' in root:
        continue
    for file in files:
        if file.endswith('.cs'):
            file_path = os.path.join(root, file)
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()

                new_content = content
                for old_val, new_val in replacements.items():
                    new_content = new_content.replace(old_val, new_val)
                    
                if new_content != content:
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(new_content)
                    print(f'Updated {file_path}')
            except Exception as e:
                print(f'Error reading {file_path}: {e}')
