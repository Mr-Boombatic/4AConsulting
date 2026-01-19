<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html" encoding="UTF-8" indent="no" omit-xml-declaration="yes"/>
    
    <xsl:template match="/TableOfContents">
        <xsl:apply-templates select="node()"/>
    </xsl:template>
    
    <xsl:template match="Chapter">
        <h1><xsl:value-of select="@title"/></h1>
        <xsl:apply-templates select="node()"/>
    </xsl:template>
    
    <xsl:template match="Section">
        <h2><xsl:value-of select="@title"/></h2>
        <xsl:apply-templates select="node()"/>
    </xsl:template>
    
    <xsl:template match="SubSection">
        <h3><xsl:value-of select="@title"/></h3>
        <xsl:apply-templates select="node()"/>
    </xsl:template>
    
    <xsl:template match="List[@type='ul']">
        <ul>
            <xsl:apply-templates select="Item"/>
        </ul>
    </xsl:template>
    
    <xsl:template match="List[@type='ol']">
        <ol>
            <xsl:apply-templates select="Item"/>
        </ol>
    </xsl:template>
    
    <xsl:template match="Item">
        <li><xsl:value-of select="."/></li>
    </xsl:template>
    
    <xsl:template match="Paragraph">
        <p>
            <xsl:apply-templates select="node()"/>
        </p>
    </xsl:template>
    
    <xsl:template match="Content">
        <xsl:copy-of select="node()"/>
    </xsl:template>
    
    <xsl:template match="text()">
        <xsl:value-of select="."/>
    </xsl:template>
</xsl:stylesheet>
