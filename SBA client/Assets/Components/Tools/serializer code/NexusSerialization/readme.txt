nexus serialiser: runtime directory system which stores variables in a fields

classes:
    nexus:parent:folder (also a directory system)
    field:child:file

m_nexus.setField("a//aa", 1); is same as m_nexus.setField("a/aa", 1);
m_nexus..LoadField_toDirectory("/a/b/c/", m_field); "/a/b/c/" is directory