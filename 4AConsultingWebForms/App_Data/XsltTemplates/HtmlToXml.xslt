<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml" encoding="UTF-8" indent="yes" omit-xml-declaration="no"/>
    
    <xsl:template match="/">
        <TableOfContents>
            <xsl:apply-templates select="html/body/node() | body/node()"/>
        </TableOfContents>
    </xsl:template>
    
    <xsl:template match="h1">
        <Chapter title="{normalize-space(.)}"/>
    </xsl:template>
    
    <xsl:template match="h2">
        <Section title="{normalize-space(.)}"/>
    </xsl:template>
    
    <xsl:template match="h3">
        <SubSection title="{normalize-space(.)}"/>
    </xsl:template>
    
    <xsl:template match="ul">
        <List type="ul">
            <xsl:apply-templates select="li"/>
        </List>
    </xsl:template>
    
    <xsl:template match="ol">
        <List type="ol">
            <xsl:apply-templates select="li"/>
        </List>
    </xsl:template>
    
    <xsl:template match="li">
        <Item>
            <xsl:value-of select="normalize-space(.)"/>
        </Item>
    </xsl:template>
    
    <xsl:template match="p">
        <Paragraph>
            <xsl:apply-templates select="node()"/>
        </Paragraph>
    </xsl:template>
    
    <xsl:template match="div | span | br | strong | em | b | i | u | a | img | table | tr | td | th | thead | tbody">
        <Content>
            <xsl:copy-of select="."/>
        </Content>
    </xsl:template>
    
    <xsl:template match="*">
        <Content>
            <xsl:copy-of select="."/>
        </Content>
    </xsl:template>
    
    <xsl:template match="text()[normalize-space(.) = '']"/>
    
    <xsl:template match="text()">
        <xsl:if test="normalize-space(.) != ''">
            <xsl:value-of select="."/>
        </xsl:if>
    </xsl:template>
</xsl:stylesheet>
