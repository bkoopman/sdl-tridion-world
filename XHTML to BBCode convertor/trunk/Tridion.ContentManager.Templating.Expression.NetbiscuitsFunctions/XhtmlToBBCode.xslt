<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:tcdl="http://www.tridion.com/ContentDelivery/5.3/TCDL" xml:space="preserve" >
  <xsl:output method="xml" version="1.0" omit-xml-declaration="yes" encoding="UTF-8" />
  <xsl:param name="IncludeImages" />
  
  <xsl:template match="tcdl:ComponentField">
    <xsl:copy>
      <xsl:for-each select="@*">
        <xsl:attribute name="{name(.)}">
          <xsl:value-of select="."/>
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates select="node()" />
    </xsl:copy>
  </xsl:template>

  <!-- Convert headings -->
  <xsl:template match="h1 | h2 | h3 | h4 | h5 | h6">
    [b]<xsl:value-of select="."/>[/b][br]
  </xsl:template>

  <!-- Convert paragraph -->
  <xsl:template match="p">
    [p]<xsl:apply-templates select="node()"/>[/p]
  </xsl:template>

  <!-- Convert line break -->
  <xsl:template match="br">
    [br]
  </xsl:template>

  <!-- Convert horizontal ruler -->
  <xsl:template match="hr">
    [hr]
  </xsl:template>

  <!-- Convert anchors -->
  <xsl:template match="a">
    <xsl:choose>
      <xsl:when test="@rel = 'nofollow'">
        [urlnofollow="<xsl:value-of select="@href" />"]<xsl:value-of select="." />[/urlnofollow]
      </xsl:when>
      <xsl:otherwise>
        [url="<xsl:value-of select="@href" />"]<xsl:value-of select="." />[/url]
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Convert images -->
  <xsl:template match="img">
    <xsl:if test="$IncludeImages">
      [img]<xsl:value-of select="@src" />[/img]
    </xsl:if>
  </xsl:template>

  <!-- Convert strong and bold tags -->
  <xsl:template match="b | strong">
    [b]<xsl:apply-templates select="node()" />[/b]
  </xsl:template>

  <!-- Convert italic text -->
  <xsl:template match="em">
    [i]<xsl:apply-templates select="node()" />[/i]
  </xsl:template>

  <!-- Convert underline text -->
  <xsl:template match="u">
    [u]<xsl:apply-templates select="node()" />[/u]
  </xsl:template>

  <!-- Convert ordered and unordered lists -->
  <xsl:template match="ul">
    [ul]<xsl:apply-templates select="node()" />[/ul]
  </xsl:template>
  <xsl:template match="ol">
    [ol]<xsl:apply-templates select="node()" />[/ol]
  </xsl:template>
  <xsl:template match="li">
    [li]<xsl:apply-templates select="node()" />[/li]
  </xsl:template>

  <!-- Convert span with id -->
  <xsl:template match="span[@id]">
    [id="<xsl:value-of select="@id" />"]<xsl:apply-templates select="node()" />[/id]
  </xsl:template>

</xsl:stylesheet>
