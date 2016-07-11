
CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
  config.uilanguage = 'it';
  config.uiColor = '#000000';
  config.toolbarCanCollapse = true;
  config.toolbarGroups = [
      { name: 'clipboard',   groups: [ 'clipboard', 'undo' ] },
      { name: 'editing',     groups: [ 'find', 'selection', 'spellchecker' ] },
      { name: 'links' },
      { name: 'insert' },
      { name: 'tools' },
      { name: 'document',    groups: [ 'mode', 'document', 'doctools' ] },
      { name: 'others' },
      '/',
      { name: 'basicstyles', groups: [ 'basicstyles', 'cleanup' ] },
      { name: 'paragraph',   groups: [ 'list', 'indent', 'blocks', 'align' ] },
      { name: 'styles' },
      { name: 'colors' },
      { name: 'about' }
  ];
};
